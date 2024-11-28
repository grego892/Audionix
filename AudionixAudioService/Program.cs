using AudionixAudioServer.Data;
using AudionixAudioServer.DataAccess;
using AudionixAudioServer.Repositories;
using AudionixAudioServer.Services;
using Microsoft.EntityFrameworkCore;
using Serilog.Settings.Configuration;
using Serilog;

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

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Debug);
builder.Logging.AddFilter("Microsoft.AspNetCore.Http.Connections", LogLevel.Debug);

// DATABASE
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Repositories
builder.Services.AddScoped<IStationRepository, StationRepository>();

// Register Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register AudioService
builder.Services.AddScoped<AudioService>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

host.Run();
