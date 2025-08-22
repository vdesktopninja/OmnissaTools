namespace LogScrub.Gui.Configuration
{
    /// <summary>
    /// Configuration settings for LogScrub application
    /// </summary>
    public class LogScrubConfiguration
    {
        /// <summary>
        /// Default anonymization settings
        /// </summary>
        public DefaultSettingsConfiguration DefaultSettings { get; set; } = new();

        /// <summary>
        /// Processing configuration
        /// </summary>
        public ProcessingConfiguration Processing { get; set; } = new();

        /// <summary>
        /// UI configuration
        /// </summary>
        public UIConfiguration UI { get; set; } = new();
    }

    /// <summary>
    /// Default settings for anonymization
    /// </summary>
    public class DefaultSettingsConfiguration
    {
        public string IpMode { get; set; } = "mask";
        public bool KeepRfc1918 { get; set; } = false;
        public bool FqdnOn { get; set; } = true;
        public bool UsersOn { get; set; } = true;
        public bool ServersOn { get; set; } = true;
    }

    /// <summary>
    /// Processing-related configuration
    /// </summary>
    public class ProcessingConfiguration
    {
        public int DefaultParallelism { get; set; } = 0; // 0 = use processor count
        public int BufferSize { get; set; } = 65536;
        public int ProgressReportIntervalMs { get; set; } = 120;
    }

    /// <summary>
    /// UI-related configuration
    /// </summary>
    public class UIConfiguration
    {
        public string DefaultInputPath { get; set; } = string.Empty;
        public string DefaultOutputPath { get; set; } = string.Empty;
        public int MaxRecentFiles { get; set; } = 10;
    }
}