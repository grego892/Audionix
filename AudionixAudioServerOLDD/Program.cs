using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Settings.Configuration;
using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Audionix.Data;
using Audionix.Models;
using Audionix.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using AudionixAudioServer.Services;

namespace AudionixAudioServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            ConfigureLogger(host.Services.GetRequiredService<IConfiguration>());
            try
            {
                Log.Information("Starting up the service");
                host.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "There was a problem starting the service");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .UseSerilog()
                .ConfigureServices((hostContext, services) =>
                {
                    var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Audionix", "Database", "Audionix.db");
                    var connectionString = $"Data Source={databasePath}";
                    services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString));

                    // Register AppSettingsService
                    services.AddSingleton<AppSettingsService>();

                    // Load AppSettings using AppSettingsService
                    var serviceProvider = services.BuildServiceProvider();
                    var appSettingsService = serviceProvider.GetRequiredService<AppSettingsService>();
                    var appSettings = appSettingsService.GetOrCreateConfigurationAsync().GetAwaiter().GetResult();

                    // Register the loaded AppSettings instance
                    services.AddSingleton(appSettings);

                    // Register IAudioPlaybackService and its implementation
                    services.AddScoped<IAudioPlaybackService, AudioPlaybackService>();

                    services.AddHostedService<Worker>();
                    services.AddScoped<AudioMetadataService>();
                    services.AddSingleton<AppStateService>();
                });

        private static void ConfigureLogger(IConfiguration configuration)
        {
            string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Audionix", "Logging", "AudionixAudioServer", "AudionixAudioServer.log");
            var options = new ConfigurationReaderOptions(typeof(Serilog.LoggerConfiguration).Assembly);
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration, options)
                .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
                .WriteTo.Console()
                .CreateLogger();
        }
    }
}
