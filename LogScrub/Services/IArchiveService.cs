using System.IO.Compression;

namespace LogScrub.Gui.Services
{
    /// <summary>
    /// Service for handling ZIP archive operations
    /// </summary>
    public interface IArchiveService
    {
        /// <summary>
        /// Extracts a ZIP archive to the specified directory
        /// </summary>
        /// <param name="zipPath">Path to ZIP file</param>
        /// <param name="extractPath">Directory to extract to</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the async operation</returns>
        Task ExtractZipAsync(string zipPath, string extractPath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a ZIP archive from the specified directory
        /// </summary>
        /// <param name="sourceDirectory">Directory to compress</param>
        /// <param name="zipPath">Output ZIP file path</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the async operation</returns>
        Task CreateZipAsync(string sourceDirectory, string zipPath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a temporary directory for extraction
        /// </summary>
        /// <param name="prefix">Prefix for temp directory name</param>
        /// <returns>Path to created temporary directory</returns>
        string CreateTempDirectory(string prefix);

        /// <summary>
        /// Extracts a ZIP archive with progress reporting
        /// </summary>
        /// <param name="zipPath">Path to ZIP file</param>
        /// <param name="extractPath">Directory to extract to</param>
        /// <param name="progress">Progress reporter</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the async operation</returns>
        Task ExtractZipWithProgressAsync(string zipPath, string extractPath, IProgress<double>? progress = null, CancellationToken cancellationToken = default);
    }
}