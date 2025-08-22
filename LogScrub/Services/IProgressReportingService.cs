namespace LogScrub.Gui.Services
{
    /// <summary>
    /// Service for handling progress reporting and ETA calculations
    /// </summary>
    public interface IProgressReportingService
    {
        /// <summary>
        /// Formats bytes into human-readable format
        /// </summary>
        /// <param name="bytes">Number of bytes</param>
        /// <returns>Formatted string (e.g., "1.2 MB")</returns>
        string FormatBytes(long bytes);

        /// <summary>
        /// Formats time span into ETA format
        /// </summary>
        /// <param name="timeSpan">Time span to format</param>
        /// <returns>Formatted ETA string</returns>
        string FormatEta(TimeSpan timeSpan);

        /// <summary>
        /// Calculates progress percentage
        /// </summary>
        /// <param name="completed">Completed bytes</param>
        /// <param name="total">Total bytes</param>
        /// <returns>Progress percentage (0-100)</returns>
        double CalculateProgress(long completed, long total);

        /// <summary>
        /// Creates a progress message with current status
        /// </summary>
        /// <param name="completed">Completed bytes</param>
        /// <param name="total">Total bytes</param>
        /// <param name="bytesPerSecond">Processing speed</param>
        /// <param name="currentFile">Currently processing file</param>
        /// <returns>Formatted progress message</returns>
        string CreateProgressMessage(long completed, long total, double bytesPerSecond, string? currentFile);
    }
}