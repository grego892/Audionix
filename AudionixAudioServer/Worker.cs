using Audionix.Shared.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace AudionixAudioServer
{
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
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var audioService = scope.ServiceProvider.GetRequiredService<AudioService>();

                    var stations = await dbContext.Stations.ToListAsync(stoppingToken);

                    foreach (var station in stations)
                    {
                        var logItem = await audioService.GetCurrentLogItemAsync(station.StationId);
                        if (logItem != null)
                        {
                            _ = Task.Run(async () =>
                            {
                                using (var innerScope = _serviceProvider.CreateScope())
                                {
                                    var innerAudioService = innerScope.ServiceProvider.GetRequiredService<AudioService>();
                                    await innerAudioService.PlayAudioAsync(logItem, stoppingToken);
                                }
                            }, stoppingToken);
                        }
                    }
                }

                Log.Information("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
