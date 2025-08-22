using Microsoft.Win32;
using Ookii.Dialogs.Wpf;              // WPF folder dialog
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using LogScrub.Gui.Services;
using LogScrub.Gui.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace LogScrub.Gui
{
    public partial class MainWindow : Window
    {
        private readonly IAnonymizationService _anonymizationService;
        private readonly IFileProcessingService _fileProcessingService;
        private readonly IArchiveService _archiveService;
        private readonly IProgressReportingService _progressReportingService;
        private readonly IValidationService _validationService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger<MainWindow> _logger;
        private readonly LogScrubConfiguration _config;
        

        public MainWindow(
            IAnonymizationService anonymizationService,
            IFileProcessingService fileProcessingService,
            IArchiveService archiveService,
            IProgressReportingService progressReportingService,
            IValidationService validationService,
            ILocalizationService localizationService,
            ILogger<MainWindow> logger,
            IOptions<LogScrubConfiguration> config,
            ViewModels.MainViewModel viewModel)
        {
            _anonymizationService = anonymizationService;
            _fileProcessingService = fileProcessingService;
            _archiveService = archiveService;
            _progressReportingService = progressReportingService;
            _validationService = validationService;
            _localizationService = localizationService;
            _logger = logger;
            _config = config.Value;
            
            InitializeComponent();
            
            // Set the DataContext to the ViewModel
            DataContext = viewModel;
            
            
            _logger.LogInformation("MainWindow initialized with MVVM");
        }

        private void BrowseInput_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new VistaFolderBrowserDialog
            {
                Description = "Select log folder",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = false
            };
            if (dlg.ShowDialog(this) == true)
            {
                var viewModel = (ViewModels.MainViewModel)DataContext;
                viewModel.InputPath = dlg.SelectedPath;
                PreviewCount(dlg.SelectedPath);
            }
        }

        private void BrowseZip_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "ZIP Archives (*.zip)|*.zip", Multiselect = false };
            if (dlg.ShowDialog() == true)
            {
                var viewModel = (ViewModels.MainViewModel)DataContext;
                viewModel.InputPath = dlg.FileName;
                AppendLog($"Selected ZIP: {dlg.FileName}");
            }
        }

        private void BrowseOutput_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = (ViewModels.MainViewModel)DataContext;
            var outIsZip = viewModel.OutputPath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase);
            if (outIsZip)
            {
                var sfd = new SaveFileDialog { Filter = "ZIP Archives (*.zip)|*.zip" };
                if (sfd.ShowDialog() == true) viewModel.OutputPath = sfd.FileName;
                return;
            }

            var dlg = new VistaFolderBrowserDialog
            {
                Description = "Select output folder",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = true
            };
            if (dlg.ShowDialog(this) == true)
                viewModel.OutputPath = dlg.SelectedPath;
        }


        // Drag & Drop
        private void Window_DragEnter(object sender, DragEventArgs e)
        {
            if (HasAnyDirOrZip(e.Data)) { e.Effects = DragDropEffects.Copy; DropHint.Visibility = Visibility.Visible; }
            else { e.Effects = DragDropEffects.None; DropHint.Visibility = Visibility.Collapsed; }
            e.Handled = true;
        }
        private void Window_DragOver(object sender, DragEventArgs e)
        {
            if (HasAnyDirOrZip(e.Data)) { e.Effects = DragDropEffects.Copy; DropHint.Visibility = Visibility.Visible; }
            else { e.Effects = DragDropEffects.None; DropHint.Visibility = Visibility.Collapsed; }
            e.Handled = true;
        }
        private void Window_DragLeave(object sender, DragEventArgs e) => DropHint.Visibility = Visibility.Collapsed;
        private void Window_Drop(object sender, DragEventArgs e)
        {
            try
            {
                DropHint.Visibility = Visibility.Collapsed;
                if (!HasAnyDirOrZip(e.Data)) return;

                var items = (string[])e.Data.GetData(DataFormats.FileDrop)!;
                var dir = items.FirstOrDefault(Directory.Exists);
                var zip = items.FirstOrDefault(p => File.Exists(p) && p.EndsWith(".zip", StringComparison.OrdinalIgnoreCase));

                var viewModel = (ViewModels.MainViewModel)DataContext;
                if (dir is not null)
                {
                    viewModel.InputPath = dir;
                    PreviewCount(dir);
                }
                else if (zip is not null)
                {
                    viewModel.InputPath = zip;
                    AppendLog($"Selected ZIP: {zip}");
                }
            }
            catch { /* ignore minor DnD errors */ }
        }
        private static bool HasAnyDirOrZip(IDataObject data)
        {
            if (!data.GetDataPresent(DataFormats.FileDrop)) return false;
            var paths = data.GetData(DataFormats.FileDrop) as string[] ?? Array.Empty<string>();
            return paths.Any(p => Directory.Exists(p) || (File.Exists(p) && p.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)));
        }

        private void AppendLog(string msg)
        {
            var viewModel = (ViewModels.MainViewModel)DataContext;
            viewModel.LogMessages.Add($"{DateTime.Now:HH:mm:ss} {msg}");
            _logger.LogInformation(msg);
        }
        private void PreviewCount(string dir)
        {
            var files = _fileProcessingService.GetTextFiles(dir);
            AppendLog($"Detected {files.Length} text files in: {dir}");
        }

    }
}
