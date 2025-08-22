using System.IO;

namespace LogScrub.Gui.Services
{
    /// <summary>
    /// Implementation of progress reporting service
    /// </summary>
    public class ProgressReportingService : IProgressReportingService
    {
        public string FormatBytes(long bytes)
        {
            string[] units = { "B", "KB", "MB", "GB", "TB" };
            double value = bytes;
            int unitIndex = 0;

            while (value >= 1024 && unitIndex < units.Length - 1)
            {
                value /= 1024;
                unitIndex++;
            }

            return $"{value:0.##} {units[unitIndex]}";
        }

        public string FormatEta(TimeSpan timeSpan)
        {
            return timeSpan.TotalHours >= 1
                ? $"{(int)timeSpan.TotalHours}h {timeSpan.Minutes}m {timeSpan.Seconds}s"
                : $"{timeSpan.Minutes}m {timeSpan.Seconds}s";
        }

        public double CalculateProgress(long completed, long total)
        {
            return total == 0 ? 0 : Math.Min(100.0, (double)completed / total * 100.0);
        }

        public string CreateProgressMessage(long completed, long total, double bytesPerSecond, string? currentFile)
        {
            var percentage = CalculateProgress(completed, total);
            var eta = bytesPerSecond > 0 ? TimeSpan.FromSeconds((total - completed) / bytesPerSecond) : TimeSpan.Zero;

            var message = $"{percentage:0.0}%  •  {FormatBytes(completed)}/{FormatBytes(total)}  •  " +
                         $"speed ~ {FormatBytes((long)bytesPerSecond)}/s  •  ETA {FormatEta(eta)}";

            if (!string.IsNullOrEmpty(currentFile))
            {
                message += $"  •  {Path.GetFileName(currentFile)}";
            }

            return message;
        }
    }
}