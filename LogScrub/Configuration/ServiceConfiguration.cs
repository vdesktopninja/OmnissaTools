using System.IO;
using LogScrub.Gui.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace LogScrub.Gui.Configuration
{
    /// <summary>
    /// Configuration for dependency injection services
    /// </summary>
    public static class ServiceConfiguration
    {
        /// <summary>
        /// Configures all services for the application
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Configuration root</param>
        /// <returns>Configured service collection</returns>
        public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configuration
            services.Configure<LogScrubConfiguration>(configuration.GetSection("LogScrub"));

            // Logging
            services.AddLogging(loggingBuilder =>
            {
                var logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(configuration)
                    .CreateLogger();

                loggingBuilder.ClearProviders();
                loggingBuilder.AddSerilog(logger);
            });

            // Application Services
            services.AddSingleton<IAnonymizationService, AnonymizationService>();
            services.AddSingleton<IFileProcessingService, FileProcessingService>();
            services.AddSingleton<IArchiveService, ArchiveService>();
            services.AddSingleton<IProgressReportingService, ProgressReportingService>();
            services.AddSingleton<IValidationService, ValidationService>();
            services.AddSingleton<ILocalizationService, LocalizationService>();

            // UI Services
            services.AddTransient<MainWindow>();
            services.AddTransient<ViewModels.MainViewModel>();

            return services;
        }

        /// <summary>
        /// Builds configuration from appsettings.json
        /// </summary>
        /// <returns>Configuration root</returns>
        public static IConfiguration BuildConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }
    }
}