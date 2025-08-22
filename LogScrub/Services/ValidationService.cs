using System.IO;
using LogScrub.Gui.Common;

namespace LogScrub.Gui.Services
{
    /// <summary>
    /// Implementation of validation service
    /// </summary>
    public class ValidationService : IValidationService
    {
        public Result ValidateInputPath(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return Result.Failure("Input path cannot be empty");
            }

            try
            {
                if (IsZipFile(path))
                {
                    return File.Exists(path) 
                        ? Result.Success() 
                        : Result.Failure($"ZIP file does not exist: {path}");
                }

                if (IsDirectory(path))
                {
                    return Directory.Exists(path) 
                        ? Result.Success() 
                        : Result.Failure($"Directory does not exist: {path}");
                }

                return Result.Failure($"Input must be a valid directory or ZIP file: {path}");
            }
            catch (Exception ex)
            {
                return Result.Failure($"Error validating input path: {ex.Message}", ex);
            }
        }

        public Result ValidateOutputPath(string? path, bool isZipOutput)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return Result.Failure("Output path cannot be empty");
            }

            try
            {
                var directory = isZipOutput ? Path.GetDirectoryName(path) : path;
                
                if (string.IsNullOrEmpty(directory))
                {
                    return Result.Failure("Invalid output path format");
                }

                // Check if we can create the directory
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // For ZIP output, validate the filename
                if (isZipOutput)
                {
                    var fileName = Path.GetFileName(path);
                    if (string.IsNullOrEmpty(fileName) || !fileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                    {
                        return Result.Failure("Output ZIP file must have a .zip extension");
                    }
                }

                return Result.Success();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Result.Failure("Access denied to output path", ex);
            }
            catch (Exception ex)
            {
                return Result.Failure($"Cannot validate output path: {ex.Message}", ex);
            }
        }

        public Result ValidateSettings(Settings? settings)
        {
            if (settings == null)
            {
                return Result.Failure("Settings cannot be null");
            }

            if (string.IsNullOrWhiteSpace(settings.IpMode))
            {
                return Result.Failure("IP mode must be specified");
            }

            if (!IsValidIpMode(settings.IpMode))
            {
                return Result.Failure($"Invalid IP mode: {settings.IpMode}. Valid modes are: mask, tokenize");
            }

            return Result.Success();
        }

        public Result ValidateParallelism(int parallelism)
        {
            if (parallelism < 1)
            {
                return Result.Failure("Parallelism must be at least 1");
            }

            if (parallelism > Environment.ProcessorCount * 2)
            {
                return Result.Failure($"Parallelism ({parallelism}) should not exceed {Environment.ProcessorCount * 2} on this system");
            }

            return Result.Success();
        }

        public bool IsZipFile(string? path)
        {
            return !string.IsNullOrEmpty(path) && 
                   File.Exists(path) && 
                   path.EndsWith(".zip", StringComparison.OrdinalIgnoreCase);
        }

        public bool IsDirectory(string? path)
        {
            return !string.IsNullOrEmpty(path) && Directory.Exists(path);
        }

        public Result<PathInfo> GetPathInfo(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return Result<PathInfo>.Failure("Path cannot be empty");
            }

            try
            {
                var isFile = File.Exists(path);
                var isDirectory = Directory.Exists(path);
                var exists = isFile || isDirectory;
                var isZip = isFile && path.EndsWith(".zip", StringComparison.OrdinalIgnoreCase);

                long? size = null;
                DateTime? lastModified = null;

                if (isFile)
                {
                    var fileInfo = new FileInfo(path);
                    size = fileInfo.Length;
                    lastModified = fileInfo.LastWriteTime;
                }
                else if (isDirectory)
                {
                    var dirInfo = new DirectoryInfo(path);
                    lastModified = dirInfo.LastWriteTime;
                }

                var pathInfo = new PathInfo(path, exists, isFile, isDirectory, isZip, size, lastModified);
                return Result<PathInfo>.Success(pathInfo);
            }
            catch (Exception ex)
            {
                return Result<PathInfo>.Failure($"Error getting path information: {ex.Message}", ex);
            }
        }

        private static bool IsValidIpMode(string ipMode)
        {
            return ipMode.ToLowerInvariant() is "mask" or "tokenize";
        }
    }
}