using AudionixAudioServer.Models;
using AudionixAudioServer.Services;
using AudionixAudioServer.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

public class Worker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public Worker(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        Log.Information("Worker starting at: {time}", DateTimeOffset.Now);
        await base.StartAsync(cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        Log.Information("Worker stopping at: {time}", DateTimeOffset.Now);
        await base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var stationRepository = scope.ServiceProvider.GetRequiredService<IStationRepository>();
                var audioService = scope.ServiceProvider.GetRequiredService<AudioService>();

                List<Station> stations = await stationRepository.GetStationsAsync();

                var tasks = new List<Task>();

                foreach (var station in stations)
                {
                    Log.Debug("---- AudioService.cs -- PlayAudioAsync() -  BEFORE tasks.Add(audioService.PlayAudioAsync(station.StationId, stoppingToken)");
                    tasks.Add(audioService.PlayAudioAsync(station.StationId, stoppingToken));
                    Log.Debug("---- AudioService.cs -- PlayAudioAsync() -  AFTER tasks.Add(audioService.PlayAudioAsync(station.StationId, stoppingToken)");

                    Log.Debug($"=================  TASKS: {tasks.Count.ToString()}");
                }

                await Task.WhenAll(tasks);
            }

            Log.Information("Worker running at: {time}", DateTimeOffset.Now);
        }
    }
}
