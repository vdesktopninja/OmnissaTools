using LogScrub.Gui;

namespace LogScrub.Gui.Services
{
    /// <summary>
    /// Service for processing files and handling file operations
    /// </summary>
    public interface IFileProcessingService
    {
        /// <summary>
        /// Gets all text files from the specified directory recursively
        /// </summary>
        /// <param name="directory">Directory to search</param>
        /// <returns>Array of text file paths</returns>
        string[] GetTextFiles(string directory);

        /// <summary>
        /// Processes a single file asynchronously
        /// </summary>
        /// <param name="srcPath">Source file path</param>
        /// <param name="baseInDir">Base input directory</param>
        /// <param name="baseOutDir">Base output directory</param>
        /// <param name="anonymizer">Anonymizer instance</param>
        /// <param name="report">Processing report</param>
        /// <param name="progress">Progress reporter</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the async operation</returns>
        Task ProcessFileAsync(string srcPath, string baseInDir, string baseOutDir,
            Anonymizer anonymizer, Report report, IProgress<ProgressUpdate> progress,
            CancellationToken cancellationToken);

        /// <summary>
        /// Calculates total size of files
        /// </summary>
        /// <param name="files">File paths</param>
        /// <returns>Total size in bytes</returns>
        long CalculateTotalSize(string[] files);
    }
}