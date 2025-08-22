using LogScrub.Gui.Common;

namespace LogScrub.Gui.Services
{
    /// <summary>
    /// Service for input validation and file system checks
    /// </summary>
    public interface IValidationService
    {
        /// <summary>
        /// Validates input path (directory or ZIP file)
        /// </summary>
        /// <param name="path">Path to validate</param>
        /// <returns>Validation result</returns>
        Result ValidateInputPath(string? path);

        /// <summary>
        /// Validates output path
        /// </summary>
        /// <param name="path">Path to validate</param>
        /// <param name="isZipOutput">Whether output should be ZIP</param>
        /// <returns>Validation result</returns>
        Result ValidateOutputPath(string? path, bool isZipOutput);

        /// <summary>
        /// Validates anonymization settings
        /// </summary>
        /// <param name="settings">Settings to validate</param>
        /// <returns>Validation result</returns>
        Result ValidateSettings(Settings? settings);

        /// <summary>
        /// Validates parallelism setting
        /// </summary>
        /// <param name="parallelism">Parallelism value to validate</param>
        /// <returns>Validation result</returns>
        Result ValidateParallelism(int parallelism);

        /// <summary>
        /// Checks if path is a ZIP file
        /// </summary>
        /// <param name="path">Path to check</param>
        /// <returns>True if path is ZIP file</returns>
        bool IsZipFile(string? path);

        /// <summary>
        /// Checks if path is a directory
        /// </summary>
        /// <param name="path">Path to check</param>
        /// <returns>True if path is directory</returns>
        bool IsDirectory(string? path);

        /// <summary>
        /// Gets detailed file system information about a path
        /// </summary>
        /// <param name="path">Path to analyze</param>
        /// <returns>Result containing path information</returns>
        Result<PathInfo> GetPathInfo(string? path);
    }

    /// <summary>
    /// Information about a file system path
    /// </summary>
    public record PathInfo(
        string Path,
        bool Exists,
        bool IsFile,
        bool IsDirectory,
        bool IsZip,
        long? Size = null,
        DateTime? LastModified = null
    );
}