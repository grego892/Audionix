using SharedLibrary.Data;
using AudionixAudioServer.DataAccess;
using AudionixAudioServer.Repositories;
using AudionixAudioServer.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Settings.Configuration;
using SharedLibrary.Repositories;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;

var builder = Host.CreateDefaultBuilder(args)
    .UseWindowsService() // Add this line to enable Windows Service
    .ConfigureServices((hostContext, services) =>
    {
        // LOGGING
        string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Audionix", "Logging", "AudioServer", "AudionixAudioServer.log");
        var configuration = hostContext.Configuration;
        var options = new ConfigurationReaderOptions(typeof(Serilog.LoggerConfiguration).Assembly);
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration, options)
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
            .WriteTo.Console()
            .CreateLogger();

        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddConsole();
            loggingBuilder.AddDebug();
            loggingBuilder.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Information);
            loggingBuilder.AddFilter("Microsoft.AspNetCore.Http.Connections", LogLevel.Information);
        });

        // Log the environment name
        var environment = hostContext.HostingEnvironment;
        Log.Information($"=== AudionixAudioServer --- Program.cs - builder.Environment.EnvironmentName:  {environment.EnvironmentName}");

        // DATABASE
        services.AddDbContextFactory<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Register DbContextFactory
        services.AddScoped<IDbContextFactory, DbContextFactory>();

        // Register Repositories
        services.AddScoped<IStationRepository, StationRepository>();
        services.AddScoped<IProgramLogRepository, ProgramLogRepository>();

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register AudioService
        services.AddScoped<AudioService>();

        services.AddHostedService<Worker>();
    });

var host = builder.Build();

host.Run();
