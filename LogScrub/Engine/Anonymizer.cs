using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace LogScrub.Gui
{
    public class Anonymizer
    {
        private readonly byte[] _hmacKey;
        private readonly Settings _s;
        private readonly Regex _userKeywordRegex;
        private readonly Regex _machineKeywordRegex;
        private readonly Regex _targetDomainRegex;

        // Default patterns
        private static readonly Regex EmailRegex = new(@"(?i)\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,63}\b", RegexOptions.Compiled);
        private static readonly Regex DomainUserRegex = new(@"(?i)\b[A-Z0-9._-]+\\[A-Z0-9.$_-]{1,64}\b", RegexOptions.Compiled);
        
        // Additional username patterns
        private static readonly Regex UsernameInQuotesRegex = new(@"(?i)\b[""']([a-z0-9._-]{3,32})[""']", RegexOptions.Compiled);
        private static readonly Regex UsernameAfterPrepositionRegex = new(@"(?i)\b(by|for|from|to|as|of)\s+([a-z][a-z0-9._-]{2,31})\b", RegexOptions.Compiled);
        private static readonly Regex UsernameInPathRegex = new(@"(?i)[\\/](users?|home|profiles?)[\\/]([a-z][a-z0-9._-]{2,31})[\\/]", RegexOptions.Compiled);
        private static readonly Regex UsernameStandaloneRegex = new(@"(?i)\b([a-z][a-z0-9._-]{4,31})(?=\s+(logged|login|authenticated|connected|access|session|failed|success))", RegexOptions.Compiled);
        
        // Username-like parameter=value and parameter: value patterns - only anonymize VALUES, preserve parameter names
        private static readonly Regex UsernameLikeParameterValueRegex = new(@"(?i)\b([a-z0-9]*_?(?:user|username|login|account|principal|identity|auth)_?[a-z0-9]*)[:\s]*[=][:\s]*([^\s,;)}\]]+)", RegexOptions.Compiled);
        private static readonly Regex UsernameLikeColonValueRegex = new(@"(?i)\b([a-z0-9]*_?(?:user|username|login|account|principal|identity|auth)_?[a-z0-9]*)[:\s]+([^\s,;)}\]]+)", RegexOptions.Compiled);
        
        // XML-like username tags: <USERNAME>value</USERNAME>, <USER>value</USER>, etc.
        private static readonly Regex UsernameLikeXmlTagRegex = new(@"(?i)<(user|username|userid|login|loginname|account|accountname|principal|identity|auth)>([^<]+)</\1>", RegexOptions.Compiled);
        
        // User/Hostname format: user/hostname, USR_TOKEN/hostname
        private static readonly Regex UserHostnameSlashRegex = new(@"(?i)\b(USR_[A-Z0-9]{8}|[A-Za-z0-9._-]+)/([A-Za-z0-9._-]+)\b", RegexOptions.Compiled);
        
        // Quoted hostname/server values: var name = "hostname", name="server01", etc.
        private static readonly Regex QuotedHostnameRegex = new(@"(?i)\b(name|hostname|server|host|machine|computer|node)\s*[=:]\s*[""']([A-Za-z0-9._-]+)[""']", RegexOptions.Compiled);
        
        // Comprehensive hardcoded keyword patterns for enhanced detection
        private static readonly string[] UserKeywords = {
            "user", "username", "userid", "uid", "upn", "subject", "sAMAccountName", 
            "principal", "account", "login", "logon", "name", "identity", "auth",
            "owner", "creator", "modifier", "actor", "client", "caller", "operator",
            "admin", "administrator", "service", "serviceaccount", "sa", "svc",
            "impersonated", "runas", "loggeduser", "currentuser", "activeuser",
            "tenant", "domain", "realm", "context", "session", "token"
        };
        
        private static readonly string[] MachineKeywords = {
            "host", "hostname", "server", "servername", "machine", "machinename", 
            "computer", "computername", "node", "nodename", "broker", "uag", "vdi",
            "endpoint", "client", "workstation", "device", "system", "source", "target",
            "gateway", "firewall", "switch", "router", "appliance", "vm", "virtual",
            "container", "pod", "cluster", "worker", "master", "agent", "daemon",
            "service", "process", "application", "app", "instance", "replica"
        };

        private static readonly Regex IPv4Regex = new(@"\b((25[0-5]|2[0-4]\d|[01]?\d\d?)\.){3}(25[0-5]|2[0-4]\d|[01]?\d\d?)\b", RegexOptions.Compiled);
        private static readonly Regex IPv6Regex = new(@"\b((?:[A-F0-9]{1,4}:){2,7}[A-F0-9]{1,4}|::1|::)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex FqdnRegex = new(@"(?i)\b(?=.{1,253}\b)(?:[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?\.)+(?:[a-z]{2,63})\b", RegexOptions.Compiled);

        // Simplified list of 2-label suffixes (abbreviated PSL)
        private static readonly HashSet<string> TwoLabelSuffix = new(StringComparer.OrdinalIgnoreCase)
        {
            "co.uk","ac.uk","gov.uk","com.au","net.au","org.au","co.jp","com.br",
            "com.pl","net.pl","org.pl","gov.pl","edu.pl"
        };

        public Anonymizer(string secretKey, Settings s)
        {
            _s = s;
            _hmacKey = SHA256.HashData(Encoding.UTF8.GetBytes(secretKey));
            
            // Build regex patterns using hardcoded comprehensive keyword lists
            var userPattern = string.Join("|", UserKeywords.Select(Regex.Escape));
            var machinePattern = string.Join("|", MachineKeywords.Select(Regex.Escape));
            
            // Enhanced patterns that catch more variations like "username:abc124", "userid=test123", etc.
            _userKeywordRegex = new Regex(
                $@"(?i)\b({userPattern})[\s]*[:=][\s]*([^\s,;)}}\\]]+)", 
                RegexOptions.Compiled);
                
            _machineKeywordRegex = new Regex(
                $@"(?i)\b({machinePattern})[\s]*[:=][\s]*([A-Za-z0-9._-]+)", 
                RegexOptions.Compiled);
            
            // Target domain regex for enhanced FQDN detection
            if (!string.IsNullOrEmpty(s.TargetDomain))
            {
                var targetDomain = s.TargetDomain.Trim();
                var escapedDomain = Regex.Escape(targetDomain);
                
                // Extract domain name without TLD (e.g., "abc" from "abc.local")
                var domainParts = targetDomain.Split('.');
                var shortDomain = domainParts[0];
                var escapedShortDomain = Regex.Escape(shortDomain);
                
                // Create comprehensive pattern that matches:
                // 1. Full domain: abc.local, server.abc.local
                // 2. Short domain: abc (when used standalone)
                var patterns = new List<string>();
                
                // Pattern for full domain and subdomains
                patterns.Add($@"[a-z0-9][a-z0-9._-]*\.{escapedDomain}");  // server.abc.local
                patterns.Add($@"{escapedDomain}");  // abc.local
                
                // Pattern for short domain (standalone, not part of another domain)
                patterns.Add($@"\b{escapedShortDomain}\b(?!\.[a-z])");  // abc but not abc.com
                
                var combinedPattern = $@"(?i)({string.Join("|", patterns)})";
                
                _targetDomainRegex = new Regex(combinedPattern, RegexOptions.Compiled);
            }
            else
            {
                _targetDomainRegex = null!;
            }
        }

        public (string Line, int IpMatches, int FqdnMatches, int UserMatches, int ServerMatches) AnonymizeLine(string line)
        {
            int ip = 0, fqdn = 0, usr = 0, srv = 0;

            // Email (UPN) - Fixed CS0165: avoid 'out var' in condition
            line = EmailRegex.Replace(line, m =>
            {
                var addr = m.Value;
                var parts = addr.Split('@');
                if (parts.Length != 2) return addr;

                var local = parts[0];
                var domain = parts[1];

                var tokUser = _s.UsersOn ? "USR_" + Token("user", local)[..8] : local;

                int incDom = 0;
                string dom = domain;
                if (_s.FqdnOn)
                {
                    dom = AnonymizeFqdn(domain, out incDom);
                    fqdn += incDom;
                }

                if (_s.UsersOn) usr++;
                return $"{tokUser}@{dom}";
            });

            // DOMAIN\user
            if (_s.UsersOn)
            {
                line = DomainUserRegex.Replace(line, m =>
                {
                    usr++; return "USR_" + Token("user", m.Value)[..8];
                });
            }

            // Enhanced user keyword detection (catches username:abc124, userid=test123, etc.)
            if (_s.UsersOn)
            {
                line = _userKeywordRegex.Replace(line, m =>
                {
                    var key = m.Groups[1].Value;
                    var val = m.Groups[2].Value;
                    usr++; 
                    return $"{key}=USR_{Token("user", val)[..8]}";
                });
            }

            // Additional username patterns for better detection
            if (_s.UsersOn)
            {
                // Usernames in quotes: "john.doe", 'admin'
                line = UsernameInQuotesRegex.Replace(line, m =>
                {
                    var user = m.Groups[1].Value;
                    if (IsLikelyUsername(user))
                    {
                        usr++;
                        var quote = m.Value[0]; // preserve original quote type
                        return $"{quote}USR_{Token("user", user)[..8]}{quote}";
                    }
                    return m.Value;
                });

                // Usernames after prepositions: "by john", "for admin", "from user123"
                line = UsernameAfterPrepositionRegex.Replace(line, m =>
                {
                    var prep = m.Groups[1].Value;
                    var user = m.Groups[2].Value;
                    if (IsLikelyUsername(user))
                    {
                        usr++;
                        return $"{prep} USR_{Token("user", user)[..8]}";
                    }
                    return m.Value;
                });

                // Usernames in file paths: /users/john/, /home/admin/
                line = UsernameInPathRegex.Replace(line, m =>
                {
                    var pathPrefix = m.Groups[1].Value;
                    var user = m.Groups[2].Value;
                    if (IsLikelyUsername(user))
                    {
                        usr++;
                        var separator = m.Value.Contains('\\') ? "\\" : "/";
                        return $"{separator}{pathPrefix}{separator}USR_{Token("user", user)[..8]}{separator}";
                    }
                    return m.Value;
                });

                // Standalone usernames before action words: "john logged in", "admin authenticated"
                line = UsernameStandaloneRegex.Replace(line, m =>
                {
                    var user = m.Groups[1].Value;
                    if (IsLikelyUsername(user))
                    {
                        usr++;
                        return $"USR_{Token("user", user)[..8]}";
                    }
                    return m.Groups[1].Value; // return just the username part if not anonymized
                });

                // Username-like parameter=value: logged_username=john, current_user=admin
                line = UsernameLikeParameterValueRegex.Replace(line, m =>
                {
                    var paramName = m.Groups[1].Value;  // Keep parameter name
                    var paramValue = m.Groups[2].Value; // Anonymize value
                    usr++;
                    return $"{paramName}=USR_{Token("userlike", paramValue)[..8]}";
                });

                // Username-like parameter: value: logged_username: john, current_user: admin
                line = UsernameLikeColonValueRegex.Replace(line, m =>
                {
                    var paramName = m.Groups[1].Value;  // Keep parameter name
                    var paramValue = m.Groups[2].Value; // Anonymize value
                    usr++;
                    return $"{paramName}: USR_{Token("userlike", paramValue)[..8]}";
                });

                // XML-like username tags: <USERNAME>user65</USERNAME> -> <USERNAME>USR_XXXXXXXX</USERNAME>
                line = UsernameLikeXmlTagRegex.Replace(line, m =>
                {
                    var tagName = m.Groups[1].Value;    // Keep tag name
                    var tagValue = m.Groups[2].Value;   // Anonymize content
                    usr++;
                    return $"<{tagName}>USR_{Token("userlike", tagValue)[..8]}</{tagName}>";
                });
            }

            // User/Hostname format: USR_PWXVB3GU/labcs03tst -> USR_PWXVB3GU/SRV_XXXXXXXX
            if (_s.ServersOn || _s.UsersOn)
            {
                line = UserHostnameSlashRegex.Replace(line, m =>
                {
                    var userPart = m.Groups[1].Value;     // Keep user part (might already be anonymized)
                    var hostPart = m.Groups[2].Value;     // Anonymize hostname
                    
                    if (_s.ServersOn)
                    {
                        srv++;
                        return $"{userPart}/SRV_{Token("server", hostPart)[..8]}";
                    }
                    return m.Value; // Return unchanged if servers anonymization is off
                });
            }

            // Quoted hostname values: var name = "hostname" -> var name = "SRV_XXXXXXXX"
            if (_s.ServersOn)
            {
                line = QuotedHostnameRegex.Replace(line, m =>
                {
                    var varName = m.Groups[1].Value;      // Keep variable name
                    var hostValue = m.Groups[2].Value;    // Anonymize hostname value
                    var quote = m.Value.Contains('"') ? '"' : '\''; // Preserve original quote type
                    
                    srv++;
                    return $"{varName} = {quote}SRV_{Token("server", hostValue)[..8]}{quote}";
                });
            }

            // Enhanced machine/server keyword detection 
            if (_s.ServersOn || _s.FqdnOn)
            {
                line = _machineKeywordRegex.Replace(line, m =>
                {
                    var key = m.Groups[1].Value;
                    var val = m.Groups[2].Value;

                    if (IPAddress.TryParse(val, out var ipAddr))
                    {
                        var repl = AnonymizeIp(ipAddr, val, out var c); ip += c;
                        return $"{key}={repl}";
                    }
                    if (_s.FqdnOn && FqdnRegex.IsMatch(val))
                    {
                        var repl = AnonymizeFqdn(val, out var c); fqdn += c;
                        return $"{key}={repl}";
                    }
                    if (_s.ServersOn)
                    {
                        srv++; return $"{key}=SRV_{Token("server", val)[..8]}";
                    }
                    return m.Value;
                });
            }

            // IP (general)
            line = IPv4Regex.Replace(line, m =>
            {
                var s = m.Value;
                if (IPAddress.TryParse(s, out var ipAddr))
                {
                    var repl = AnonymizeIp(ipAddr, s, out var c); ip += c;
                    return repl;
                }
                return s;
            });
            line = IPv6Regex.Replace(line, m =>
            {
                var s = m.Value;
                if (IPAddress.TryParse(s, out var ipAddr))
                {
                    var repl = AnonymizeIp(ipAddr, s, out var c); ip += c;
                    return repl;
                }
                return s;
            });

            // Target domain FQDN detection (prioritized) - fully anonymize
            if (_s.FqdnOn && _targetDomainRegex != null)
            {
                line = _targetDomainRegex.Replace(line, m =>
                {
                    var s = m.Value;
                    var repl = AnonymizeTargetDomainFqdn(s, out var c); fqdn += c;
                    return repl;
                });
            }

            // FQDN (general) - preserve public domains
            if (_s.FqdnOn)
            {
                line = FqdnRegex.Replace(line, m =>
                {
                    var s = m.Value;
                    // Skip if this matches target domain (already processed above)
                    if (_targetDomainRegex != null && _targetDomainRegex.IsMatch(s))
                        return s;
                    var repl = AnonymizeFqdn(s, out var c); fqdn += c;
                    return repl;
                });
            }

            return (line, ip, fqdn, usr, srv);
        }

        private string AnonymizeIp(IPAddress addr, string original, out int count)
        {
            count = 0;
            if (_s.KeepRfc1918 && IsPrivate(addr)) return original;

            count = 1;
            return _s.IpMode == "tokenize"
                ? "IP_" + Token("ip", addr.ToString())[..10]
                : MaskIp(original, addr);
        }

        private static string MaskIp(string original, IPAddress ip)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                var oct = original.Split('.');
                if (oct.Length == 4) return $"{oct[0]}.***.***.{oct[3]}";
            }
            else
            {
                var parts = original.Split(':');
                if (parts.Length >= 3) return $"{parts[0]}:****::{parts[^1]}";
            }
            return "IP_MASKED";
        }

        private static bool IsPrivate(IPAddress ip)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                var b = ip.GetAddressBytes();
                if (b[0] == 10) return true;
                if (b[0] == 172 && b[1] >= 16 && b[1] <= 31) return true;
                if (b[0] == 192 && b[1] == 168) return true;
                return false;
            }
            else
            {
                var b = ip.GetAddressBytes();
                return (b[0] & 0xFE) == 0xFC || (b[0] == 0xFE && (b[1] & 0xC0) == 0x80); // ULA + link-local
            }
        }

        private string AnonymizeFqdn(string fqdn, out int count)
        {
            count = 0;
            var labels = fqdn.Split('.');
            if (labels.Length < 2) return fqdn;

            int sufLen = 1;
            if (labels.Length >= 2)
            {
                var last2 = $"{labels[^2]}.{labels[^1]}";
                if (TwoLabelSuffix.Contains(last2)) sufLen = 2;
            }

            var suffix = string.Join('.', labels[^sufLen..]);
            var left = labels[..(labels.Length - sufLen)];
            if (left.Length == 0) return suffix;

            for (int i = 0; i < left.Length; i++)
                left[i] = Token("fqdn", left[i] + "|" + suffix)[..2]; // short labels
            count = 1;
            return string.Join('.', left) + "." + suffix;
        }

        private string AnonymizeTargetDomainFqdn(string domainMatch, out int count)
        {
            count = 1;
            
            // Determine if this is a short domain name or full domain
            bool isShortDomain = !domainMatch.Contains('.');
            
            if (isShortDomain)
            {
                // For short domain names (e.g., "abc"), return a short token
                return "DMN_" + Token("target_domain_short", domainMatch)[..6];
            }
            else
            {
                // For full domain names (e.g., "abc.local", "server.abc.local"), return FQDN format
                return "FQDN_" + Token("target_fqdn", domainMatch)[..8] + ".com";
            }
        }

        private string Token(string type, string value)
        {
            using var h = new HMACSHA256(_hmacKey);
            var bytes = h.ComputeHash(Encoding.UTF8.GetBytes(type + "|" + value));
            return Base32.Encode(bytes);
        }

        private static readonly HashSet<string> CommonWords = new(StringComparer.OrdinalIgnoreCase)
        {
            // Common words that are not usernames
            "admin", "administrator", "root", "system", "service", "guest", "public", "default",
            "user", "users", "test", "temp", "local", "domain", "server", "client", "host",
            "email", "mail", "web", "www", "ftp", "ssh", "http", "https", "api", "app",
            "data", "database", "file", "folder", "directory", "path", "home", "profile",
            "config", "configuration", "setting", "settings", "log", "logs", "backup",
            "cache", "session", "token", "key", "password", "pass", "secret", "secure",
            "network", "internet", "connection", "access", "permission", "security",
            "application", "program", "process", "thread", "service", "daemon",
            "windows", "linux", "unix", "microsoft", "google", "apple", "oracle"
        };

        private static bool IsLikelyUsername(string candidate)
        {
            // Filter out obvious non-usernames
            if (string.IsNullOrWhiteSpace(candidate) || candidate.Length < 3 || candidate.Length > 32)
                return false;

            // Skip common system words
            if (CommonWords.Contains(candidate))
                return false;

            // Skip if it looks like a file extension or URL
            if (candidate.Contains('.') && (candidate.EndsWith(".exe") || candidate.EndsWith(".dll") || 
                candidate.EndsWith(".log") || candidate.EndsWith(".txt") || candidate.EndsWith(".com")))
                return false;

            // Skip if it's all numbers (likely an ID, not username)
            if (candidate.All(char.IsDigit))
                return false;

            // Skip very short words unless they look like usernames
            if (candidate.Length < 4 && !candidate.Any(char.IsDigit))
                return false;

            return true;
        }
    }

    internal static class Base32
    {
        private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        public static string Encode(byte[] data)
        {
            if (data.Length == 0) return string.Empty;
            int outputLength = (int)Math.Ceiling(data.Length / 5d) * 8;
            var result = new StringBuilder(outputLength);

            int buffer = data[0];
            int next = 1;
            int bitsLeft = 8;
            while (bitsLeft > 0 || next < data.Length)
            {
                if (bitsLeft < 5)
                {
                    if (next < data.Length)
                    {
                        buffer <<= 8;
                        buffer |= data[next++] & 0xff;
                        bitsLeft += 8;
                    }
                    else
                    {
                        int pad = 5 - bitsLeft;
                        buffer <<= pad;
                        bitsLeft += pad;
                    }
                }
                int index = 0x1f & (buffer >> (bitsLeft - 5));
                bitsLeft -= 5;
                result.Append(Alphabet[index]);
            }
            return result.ToString();
        }
    }
}
