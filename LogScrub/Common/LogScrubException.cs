namespace LogScrub.Gui.Common
{
    /// <summary>
    /// Base exception for LogScrub application errors
    /// </summary>
    public class LogScrubException : Exception
    {
        public LogScrubException(string message) : base(message) { }
        public LogScrubException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown during file processing operations
    /// </summary>
    public class FileProcessingException : LogScrubException
    {
        public string? FilePath { get; }

        public FileProcessingException(string message) : base(message) { }
        public FileProcessingException(string message, string filePath) : base(message)
        {
            FilePath = filePath;
        }
        public FileProcessingException(string message, Exception innerException) : base(message, innerException) { }
        public FileProcessingException(string message, string filePath, Exception innerException) : base(message, innerException)
        {
            FilePath = filePath;
        }
    }

    /// <summary>
    /// Exception thrown during anonymization operations
    /// </summary>
    public class AnonymizationException : LogScrubException
    {
        public AnonymizationException(string message) : base(message) { }
        public AnonymizationException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown during validation operations
    /// </summary>
    public class ValidationException : LogScrubException
    {
        public ValidationException(string message) : base(message) { }
        public ValidationException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown during archive operations
    /// </summary>
    public class ArchiveException : LogScrubException
    {
        public string? ArchivePath { get; }

        public ArchiveException(string message) : base(message) { }
        public ArchiveException(string message, string archivePath) : base(message)
        {
            ArchivePath = archivePath;
        }
        public ArchiveException(string message, Exception innerException) : base(message, innerException) { }
        public ArchiveException(string message, string archivePath, Exception innerException) : base(message, innerException)
        {
            ArchivePath = archivePath;
        }
    }
}