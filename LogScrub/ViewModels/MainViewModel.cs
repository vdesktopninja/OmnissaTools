using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using LogScrub.Gui.Common;
using LogScrub.Gui.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using LogScrub.Gui.Configuration;

namespace LogScrub.Gui.ViewModels
{
    /// <summary>
    /// Main view model for the LogScrub application
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly IAnonymizationService _anonymizationService;
        private readonly IFileProcessingService _fileProcessingService;
        private readonly IArchiveService _archiveService;
        private readonly IProgressReportingService _progressReportingService;
        private readonly IValidationService _validationService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger<MainViewModel> _logger;
        private readonly LogScrubConfiguration _config;

        private string _inputPath = string.Empty;
        private string _outputPath = string.Empty;
        private bool _anonymizeIPs = true;
        private string _ipMode = "mask";
        private bool _keepRfc1918;
        private bool _anonymizeFqdns = true;
        private bool _anonymizeUsers = true;
        private bool _anonymizeServers = true;
        private double _parallelism = Environment.ProcessorCount;
        private bool _isProcessing;
        private double _progressPercentage;
        private string _progressMessage = string.Empty;
        private bool _canOpenOutput;
        private string _targetDomain = string.Empty;

        private CancellationTokenSource? _cancellationTokenSource;

        public MainViewModel(
            IAnonymizationService anonymizationService,
            IFileProcessingService fileProcessingService,
            IArchiveService archiveService,
            IProgressReportingService progressReportingService,
            IValidationService validationService,
            ILocalizationService localizationService,
            ILogger<MainViewModel> logger,
            IOptions<LogScrubConfiguration> config)
        {
            _anonymizationService = anonymizationService;
            _fileProcessingService = fileProcessingService;
            _archiveService = archiveService;
            _progressReportingService = progressReportingService;
            _validationService = validationService;
            _localizationService = localizationService;
            _logger = logger;
            _config = config.Value;

            InitializeCommands();
            InitializeSettings();
            LogMessages = new ObservableCollection<string>();
        }

        #region Properties

        public string InputPath
        {
            get => _inputPath;
            set => SetProperty(ref _inputPath, value, UpdateOutputPathIfEmpty);
        }

        public string OutputPath
        {
            get => _outputPath;
            set => SetProperty(ref _outputPath, value);
        }

        public bool AnonymizeIPs
        {
            get => _anonymizeIPs;
            set => SetProperty(ref _anonymizeIPs, value);
        }

        public string IpMode
        {
            get => _ipMode;
            set => SetProperty(ref _ipMode, value);
        }

        public bool KeepRfc1918
        {
            get => _keepRfc1918;
            set => SetProperty(ref _keepRfc1918, value);
        }

        public bool AnonymizeFqdns
        {
            get => _anonymizeFqdns;
            set => SetProperty(ref _anonymizeFqdns, value);
        }

        public bool AnonymizeUsers
        {
            get => _anonymizeUsers;
            set => SetProperty(ref _anonymizeUsers, value);
        }

        public bool AnonymizeServers
        {
            get => _anonymizeServers;
            set => SetProperty(ref _anonymizeServers, value);
        }

        public double Parallelism
        {
            get => _parallelism;
            set => SetProperty(ref _parallelism, value);
        }

        public bool IsProcessing
        {
            get => _isProcessing;
            set => SetProperty(ref _isProcessing, value, () =>
            {
                OnPropertyChanged(nameof(CanStartProcessing));
                OnPropertyChanged(nameof(CanCancelProcessing));
            });
        }

        public double ProgressPercentage
        {
            get => _progressPercentage;
            set => SetProperty(ref _progressPercentage, value);
        }

        public string ProgressMessage
        {
            get => _progressMessage;
            set => SetProperty(ref _progressMessage, value);
        }

        public bool CanOpenOutput
        {
            get => _canOpenOutput;
            set => SetProperty(ref _canOpenOutput, value);
        }

        public bool CanStartProcessing => !IsProcessing;
        public bool CanCancelProcessing => IsProcessing;

        public ObservableCollection<string> LogMessages { get; }

        public List<string> AvailableIpModes { get; } = new() { "mask", "tokenize" };

        public string TargetDomain
        {
            get => _targetDomain;
            set => SetProperty(ref _targetDomain, value);
        }

        #endregion

        #region Commands

        public ICommand BrowseInputCommand { get; private set; } = null!;
        public ICommand BrowseZipCommand { get; private set; } = null!;
        public ICommand BrowseOutputCommand { get; private set; } = null!;
        public ICommand StartProcessingCommand { get; private set; } = null!;
        public ICommand CancelProcessingCommand { get; private set; } = null!;
        public ICommand OpenOutputCommand { get; private set; } = null!;

        #endregion

        #region Private Methods

        private void InitializeCommands()
        {
            BrowseInputCommand = new RelayCommand(BrowseInput, () => CanStartProcessing);
            BrowseZipCommand = new RelayCommand(BrowseZip, () => CanStartProcessing);
            BrowseOutputCommand = new RelayCommand(BrowseOutput, () => CanStartProcessing);
            StartProcessingCommand = new RelayCommand(async () => await StartProcessingAsync(), () => CanStartProcessing);
            CancelProcessingCommand = new RelayCommand(CancelProcessing, () => CanCancelProcessing);
            OpenOutputCommand = new RelayCommand(OpenOutput, () => CanOpenOutput);
        }

        private void InitializeSettings()
        {
            var defaultParallelism = _config.Processing.DefaultParallelism > 0 
                ? _config.Processing.DefaultParallelism 
                : Environment.ProcessorCount;
            Parallelism = defaultParallelism;

            _logger.LogInformation("MainViewModel initialized");
        }

        private void BrowseInput()
        {
            // This would typically use a dialog service
            // For now, keep the existing dialog logic in the view
        }

        private void BrowseZip()
        {
            // This would typically use a dialog service
            // For now, keep the existing dialog logic in the view
        }

        private void BrowseOutput()
        {
            // This would typically use a dialog service
            // For now, keep the existing dialog logic in the view
        }

        private async Task StartProcessingAsync()
        {
            try
            {
                // Validate inputs
                var inputValidation = _validationService.ValidateInputPath(InputPath);
                if (!inputValidation.IsSuccess)
                {
                    AppendLog($"Input validation failed: {inputValidation.Error}");
                    return;
                }

                var outputIsZip = !string.IsNullOrEmpty(OutputPath) && OutputPath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase);
                var outputValidation = _validationService.ValidateOutputPath(OutputPath, outputIsZip);
                if (!outputValidation.IsSuccess)
                {
                    AppendLog($"Output validation failed: {outputValidation.Error}");
                    return;
                }

                var settings = new Settings
                {
                    IpMode = IpMode,
                    KeepRfc1918 = KeepRfc1918,
                    FqdnOn = AnonymizeFqdns,
                    UsersOn = AnonymizeUsers,
                    ServersOn = AnonymizeServers,
                    TargetDomain = string.IsNullOrWhiteSpace(TargetDomain) ? null : TargetDomain.Trim()
                };

                var settingsValidation = _validationService.ValidateSettings(settings);
                if (!settingsValidation.IsSuccess)
                {
                    AppendLog($"Settings validation failed: {settingsValidation.Error}");
                    return;
                }

                var parallelismValidation = _validationService.ValidateParallelism((int)Parallelism);
                if (!parallelismValidation.IsSuccess)
                {
                    AppendLog($"Parallelism validation failed: {parallelismValidation.Error}");
                    return;
                }

                IsProcessing = true;
                _cancellationTokenSource = new CancellationTokenSource();
                ProgressPercentage = 0;
                LogMessages.Clear();

                _logger.LogInformation("Starting anonymization process. Input: {Input}, Output: {Output}", InputPath, OutputPath);

                var secret = Guid.NewGuid().ToString("N");
                var anonymizer = _anonymizationService.CreateAnonymizer(secret, settings);

                AppendLog("Processing started...");

                var isZipInput = _validationService.IsZipFile(InputPath);

                string? tempExtractDir = null;
                string? tempOutDir = null;

                try
                {
                    string workInDir;
                    string workOutDir;

                    if (isZipInput)
                    {
                        tempExtractDir = _archiveService.CreateTempDirectory("LogScrub_Extract");
                        AppendLog($"Extracting ZIP to: {tempExtractDir}");
                        await _archiveService.ExtractZipAsync(InputPath, tempExtractDir);

                        workInDir = tempExtractDir;
                        workOutDir = outputIsZip
                            ? (tempOutDir = _archiveService.CreateTempDirectory("LogScrub_Out"))
                            : OutputPath;
                        Directory.CreateDirectory(workOutDir);
                    }
                    else
                    {
                        workInDir = InputPath;
                        workOutDir = OutputPath;
                        Directory.CreateDirectory(workOutDir);
                    }

                    var files = _fileProcessingService.GetTextFiles(workInDir);

                    if (files.Length == 0)
                    {
                        AppendLog("No text files to process.");
                        return;
                    }

                    var totalBytes = _fileProcessingService.CalculateTotalSize(files);
                    var processedBytes = 0L;
                    var report = new Report();

                    var progress = new Progress<ProgressUpdate>(update =>
                    {
                        Interlocked.Add(ref processedBytes, update.BytesDelta);
                        ProgressPercentage = _progressReportingService.CalculateProgress(processedBytes, totalBytes);
                        ProgressMessage = _progressReportingService.CreateProgressMessage(processedBytes, totalBytes, 0, update.FileName);
                    });

                    var parallelOptions = new ParallelOptions 
                    { 
                        MaxDegreeOfParallelism = (int)Parallelism,
                        CancellationToken = _cancellationTokenSource.Token 
                    };

                    await Parallel.ForEachAsync(files, parallelOptions, async (file, ct) =>
                    {
                        try
                        {
                            await _fileProcessingService.ProcessFileAsync(file, workInDir, workOutDir, anonymizer, report, progress, ct);
                        }
                        catch (OperationCanceledException)
                        {
                            _logger.LogInformation("File processing cancelled for {File}", file);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error processing file {File}: {Message}", file, ex.Message);
                            AppendLog($"[WARN] {Path.GetFileName(file)}: {ex.Message}");
                            Interlocked.Increment(ref report.Errors);
                        }
                    });

                    AppendLog($"[OK] Processed. Files: {report.FilesProcessed}, Skipped(bin): {report.SkippedNonText}, Errors: {report.Errors}");
                    AppendLog($"     IP:{report.IpCount} FQDN:{report.FqdnCount} Users:{report.UserCount} Servers:{report.ServerCount}");

                    if (outputIsZip)
                    {
                        AppendLog($"Packing result to ZIP: {OutputPath}");
                        await _archiveService.CreateZipAsync(workOutDir, OutputPath);
                    }

                    ProgressPercentage = 100;
                    ProgressMessage = "Processing completed";
                    CanOpenOutput = true;
                    AppendLog("Processing completed successfully");
                }
                finally
                {
                    // Cleanup temporary directories
                    if (tempExtractDir is not null)
                    {
                        try
                        {
                            if (Directory.Exists(tempExtractDir))
                            {
                                Directory.Delete(tempExtractDir, true);
                                _logger.LogDebug("Cleaned up temporary extraction directory: {Path}", tempExtractDir);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to cleanup temporary extraction directory: {Path}", tempExtractDir);
                        }
                    }

                    if (tempOutDir is not null)
                    {
                        try
                        {
                            if (Directory.Exists(tempOutDir))
                            {
                                Directory.Delete(tempOutDir, true);
                                _logger.LogDebug("Cleaned up temporary output directory: {Path}", tempOutDir);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to cleanup temporary output directory: {Path}", tempOutDir);
                        }
                    }
                }

                _logger.LogInformation("Processing completed successfully");
            }
            catch (OperationCanceledException)
            {
                AppendLog("Processing cancelled");
                _logger.LogInformation("Processing was cancelled");
            }
            catch (Exception ex)
            {
                AppendLog($"Error: {ex.Message}");
                _logger.LogError(ex, "Error during processing");
            }
            finally
            {
                IsProcessing = false;
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        private void CancelProcessing()
        {
            _cancellationTokenSource?.Cancel();
            AppendLog("Cancelling processing...");
        }

        private void OpenOutput()
        {
            if (File.Exists(OutputPath))
            {
                Process.Start("explorer.exe", $"/select,\"{OutputPath}\"");
            }
            else if (Directory.Exists(OutputPath))
            {
                Process.Start("explorer.exe", OutputPath);
            }
        }

        private void UpdateOutputPathIfEmpty()
        {
            if (string.IsNullOrWhiteSpace(OutputPath) && !string.IsNullOrWhiteSpace(InputPath))
            {
                var isZipInput = _validationService.IsZipFile(InputPath);
                OutputPath = isZipInput
                    ? Path.Combine(Path.GetDirectoryName(InputPath)!,
                        Path.GetFileNameWithoutExtension(InputPath) + "_redacted.zip")
                    : Path.Combine(InputPath, "redacted");
            }
        }

        private void AppendLog(string message)
        {
            var timestampedMessage = $"{DateTime.Now:HH:mm:ss} {message}";
            
            // Update on UI thread
            Application.Current.Dispatcher.Invoke(() =>
            {
                LogMessages.Add(timestampedMessage);
                
                // Limit log messages to prevent memory issues
                while (LogMessages.Count > 1000)
                {
                    LogMessages.RemoveAt(0);
                }
            });

            _logger.LogInformation(message);
        }


        #endregion
    }
}