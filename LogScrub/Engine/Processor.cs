using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LogScrub.Gui
{
    public static class Processor
    {
        public static async Task ProcessFileAsync(
            string srcPath, string baseInDir, string baseOutDir,
            Anonymizer anonymizer, Report report,
            IProgress<ProgressUpdate> progress, CancellationToken ct)
        {
            var rel = Path.GetRelativePath(baseInDir, srcPath);
            var dstPath = Path.Combine(baseOutDir, rel);
            Directory.CreateDirectory(Path.GetDirectoryName(dstPath)!);

            if (!FileHelpers.IsLikelyTextByExtension(srcPath) || FileHelpers.LooksBinaryHead(srcPath, 8192))
            {
                Interlocked.Increment(ref report.SkippedNonText);
                return;
            }

            var (enc, _) = FileHelpers.DetectEncoding(srcPath);
            enc ??= new UTF8Encoding(false);

            using var inFs = new FileStream(srcPath, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 16, FileOptions.SequentialScan);
            using var reader = new StreamReader(inFs, enc, detectEncodingFromByteOrderMarks: true);
            using var outFs = new FileStream(dstPath, FileMode.Create, FileAccess.Write, FileShare.None, 1 << 16, FileOptions.SequentialScan);
            using var writer = new StreamWriter(outFs, new UTF8Encoding(false));

            string? line;
            long lastBytesReported = 0;
            var lastReport = DateTime.UtcNow;

            long ip = 0, fqdn = 0, usr = 0, srv = 0;

            while ((line = await reader.ReadLineAsync()) is not null)
            {
                ct.ThrowIfCancellationRequested();
                var res = anonymizer.AnonymizeLine(line);
                await writer.WriteLineAsync(res.Line);
                ip += res.IpMatches; fqdn += res.FqdnMatches; usr += res.UserMatches; srv += res.ServerMatches;

                if ((DateTime.UtcNow - lastReport).TotalMilliseconds >= 120)
                {
                    long delta = inFs.Position - lastBytesReported;
                    if (delta > 0)
                    {
                        progress.Report(new ProgressUpdate { BytesDelta = delta, FileName = srcPath });
                        lastBytesReported = inFs.Position; lastReport = DateTime.UtcNow;
                    }
                }
            }
            await writer.FlushAsync();

            long finalDelta = inFs.Position - lastBytesReported;
            if (finalDelta > 0) progress.Report(new ProgressUpdate { BytesDelta = finalDelta, FileName = srcPath });

            Interlocked.Add(ref report.IpCount, ip);
            Interlocked.Add(ref report.FqdnCount, fqdn);
            Interlocked.Add(ref report.UserCount, usr);
            Interlocked.Add(ref report.ServerCount, srv);
            Interlocked.Increment(ref report.FilesProcessed);
        }
    }

    public struct ProgressUpdate
    {
        public long BytesDelta;
        public string? FileName;
    }
}
