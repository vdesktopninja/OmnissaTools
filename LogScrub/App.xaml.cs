using System.Windows;
using LogScrub.Gui.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LogScrub.Gui
{
    /// <summary>
    /// Application entry point with dependency injection setup
    /// </summary>
    public partial class App : Application
    {
        private IHost? _host;
        private ILogger<App>? _logger;

        protected override async void OnStartup(StartupEventArgs e)
        {
            try
            {
                // Build configuration
                var configuration = ServiceConfiguration.BuildConfiguration();

                // Build host with DI
                _host = Host.CreateDefaultBuilder()
                    .ConfigureServices((context, services) => services.ConfigureServices(configuration))
                    .Build();

                await _host.StartAsync();

                // Get logger
                _logger = _host.Services.GetRequiredService<ILogger<App>>();
                _logger.LogInformation("LogScrub application started");

                // Create and show main window
                var mainWindow = _host.Services.GetRequiredService<MainWindow>();
                mainWindow.Show();

                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start application: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(1);
            }
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            _logger?.LogInformation("LogScrub application shutting down");

            if (_host != null)
            {
                await _host.StopAsync();
                _host.Dispose();
            }

            base.OnExit(e);
        }
    }
}
