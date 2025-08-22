using System.IO;
using LogScrub.Gui;

namespace LogScrub.Gui.Services
{
    /// <summary>
    /// Implementation of file processing service
    /// </summary>
    public class FileProcessingService : IFileProcessingService
    {
        public string[] GetTextFiles(string directory)
        {
            return Directory.EnumerateFiles(directory, "*", new EnumerationOptions
            {
                RecurseSubdirectories = true,
                IgnoreInaccessible = true,
                AttributesToSkip = FileAttributes.System | FileAttributes.ReparsePoint
            })
            .Where(FileHelpers.IsLikelyTextByExtension)
            .Where(p => !FileHelpers.LooksBinaryHead(p, 8192))
            .ToArray();
        }

        public async Task ProcessFileAsync(string srcPath, string baseInDir, string baseOutDir,
            Anonymizer anonymizer, Report report, IProgress<ProgressUpdate> progress,
            CancellationToken cancellationToken)
        {
            await Processor.ProcessFileAsync(srcPath, baseInDir, baseOutDir, anonymizer, report, progress, cancellationToken);
        }

        public long CalculateTotalSize(string[] files)
        {
            return files.Select(f => new FileInfo(f).Length).Sum();
        }
    }
}