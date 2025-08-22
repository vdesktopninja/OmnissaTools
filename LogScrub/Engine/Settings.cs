namespace LogScrub.Gui
{
    /// <summary>
    /// Configuration settings for the anonymization process
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// IP anonymization mode: "mask" to partially hide IPs, "tokenize" to replace with tokens
        /// </summary>
        public string IpMode { get; set; } = "mask"; // mask | tokenize

        /// <summary>
        /// Whether to keep RFC1918 private IP addresses unchanged
        /// </summary>
        public bool KeepRfc1918 { get; set; } = false;

        /// <summary>
        /// Whether to anonymize Fully Qualified Domain Names (FQDNs)
        /// </summary>
        public bool FqdnOn { get; set; } = true;

        /// <summary>
        /// Whether to anonymize user names and email addresses
        /// </summary>
        public bool UsersOn { get; set; } = true;

        /// <summary>
        /// Whether to anonymize server and host names
        /// </summary>
        public bool ServersOn { get; set; } = true;

        /// <summary>
        /// Optional specific domain to prioritize for FQDN anonymization (e.g., "company.com")
        /// </summary>
        public string? TargetDomain { get; set; } = null;
    }
}
