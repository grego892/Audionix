using AudionixAudioServer.Services;
using Serilog.Settings.Configuration;
using Serilog;
using AudionixAudioServer.Data;
using AudionixAudioServer.Repositories;
using AudionixAudioServer.DataAccess;
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
