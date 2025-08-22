using System.IO;
using System.IO.Compression;

namespace LogScrub.Gui.Services
{
    /// <summary>
    /// Implementation of archive service for ZIP operations
    /// </summary>
    public class ArchiveService : IArchiveService
    {
        public async Task ExtractZipAsync(string zipPath, string extractPath, CancellationToken cancellationToken = default)
        {
            await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                ZipFile.ExtractToDirectory(zipPath, extractPath);
            }, cancellationToken);
        }

        public async Task CreateZipAsync(string sourceDirectory, string zipPath, CancellationToken cancellationToken = default)
        {
            await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                if (File.Exists(zipPath))
                    File.Delete(zipPath);

                ZipFile.CreateFromDirectory(sourceDirectory, zipPath, 
                    CompressionLevel.Optimal, includeBaseDirectory: false);
            }, cancellationToken);
        }

        public string CreateTempDirectory(string prefix)
        {
            var tempPath = Path.Combine(Path.GetTempPath(), $"{prefix}_{Guid.NewGuid():N}");
            Directory.CreateDirectory(tempPath);
            return tempPath;
        }

        public async Task ExtractZipWithProgressAsync(string zipPath, string extractPath, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
        {
            await Task.Run(() =>
            {
                using var archive = ZipFile.OpenRead(zipPath);
                var totalEntries = archive.Entries.Count;
                var extractedEntries = 0;

                foreach (var entry in archive.Entries)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var destinationPath = Path.Combine(extractPath, entry.FullName);
                    
                    // Create directory if it doesn't exist
                    var directoryPath = Path.GetDirectoryName(destinationPath);
                    if (!string.IsNullOrEmpty(directoryPath))
                        Directory.CreateDirectory(directoryPath);

                    // Extract file
                    if (!string.IsNullOrEmpty(entry.Name))
                        entry.ExtractToFile(destinationPath, overwrite: true);

                    extractedEntries++;
                    progress?.Report((double)extractedEntries / totalEntries * 100);
                }
            }, cancellationToken);
        }
    }
}