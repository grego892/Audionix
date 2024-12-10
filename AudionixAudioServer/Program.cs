using SharedLibrary.Data;
using AudionixAudioServer.DataAccess;
using AudionixAudioServer.Repositories;
using AudionixAudioServer.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Settings.Configuration;
using SharedLibrary.Repositories;


var builder = Host.CreateApplicationBuilder(args);

// Load configuration
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

// LOGGING
string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Audionix", "Logging", "AudioServer", "AudionixAudioServer.log");
var configuration = builder.Configuration;
var options = new ConfigurationReaderOptions(typeof(Serilog.LoggerConfiguration).Assembly);
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration, options)
    .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
    .WriteTo.Console()
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Information);
builder.Logging.AddFilter("Microsoft.AspNetCore.Http.Connections", LogLevel.Information);

Log.Information("=== AudionixAudioServer --- Program.cs - builder.Environment.EnvironmentName:  " + builder.Environment.EnvironmentName);
var assembly = System.Reflection.Assembly.GetExecutingAssembly();
Log.Information("--- Program.cs - Running version: " + assembly.GetName().Version);

// DATABASE
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register DbContextFactory
builder.Services.AddScoped<IDbContextFactory, DbContextFactory>();

// Register Repositories
builder.Services.AddScoped<IStationRepository, StationRepository>();
builder.Services.AddScoped<IProgramLogRepository, ProgramLogRepository>();

// Register Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register AudioService
builder.Services.AddScoped<AudioService>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

host.Run();
