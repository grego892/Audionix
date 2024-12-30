using SharedLibrary.Data;
using SharedLibrary.Repositories;
using AudionixAudioServer.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Settings.Configuration;

var builder = Host.CreateDefaultBuilder(args)
    .UseWindowsService()
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