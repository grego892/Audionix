<<<<<<< HEAD
using AudionixAudioServer;
using AudionixAudioServer.Services;
using Serilog.Settings.Configuration;
using Serilog;
using Audionix.Shared.Data;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

// LOGGING
string _logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Audionix", "Logging", "AudioServer", "AudionixAudioServer.log");
var configuration = builder.Configuration;
var options = new ConfigurationReaderOptions(typeof(Serilog.LoggerConfiguration).Assembly);
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration, options)
    .WriteTo.File(Path.Combine(_logPath), rollingInterval: RollingInterval.Day)
    .WriteTo.Console()
    .CreateLogger();
//

// DATABASE
var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Audionix", "Database", "Audionix.db");
var databaseDirectory = Path.GetDirectoryName(databasePath);
if (string.IsNullOrEmpty(databaseDirectory))
{
    Log.Error("--- Program.cs - Database directory path is null or empty.");
    throw new ArgumentNullException(nameof(databaseDirectory), "Database directory path cannot be null or empty.");
}
Directory.CreateDirectory(databaseDirectory);
var connectionString = $"Data Source={databasePath}";
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString));
//

// ConfigurationService
builder.Services.AddSingleton<ConfigurationService>();

builder.Services.AddScoped<AudioService>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

host.Run();
=======
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
>>>>>>> 27d7ef494a86774933472634846363b9f0ad13ea
