using AudionixAudioServer;
using AudionixAudioServer.Services;
using Serilog.Settings.Configuration;
using Serilog;
using Audionix.Shared.Data;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

// LOGGING
builder.Configuration.AddJsonFile("appsettingsAudioServer.json", optional: false, reloadOnChange: true);

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
