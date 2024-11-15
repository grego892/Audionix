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
            using (var scope = _serviceProvider.CreateScope())
            {
                var audioService = scope.ServiceProvider.GetRequiredService<AudioService>();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                while (!stoppingToken.IsCancellationRequested)
                {
                    var stations = await dbContext.Stations.ToListAsync(stoppingToken);

                    foreach (var station in stations)
                    {
                        var logItem = await audioService.GetCurrentLogItemAsync(station.StationId);
                        if (logItem != null)
                        {
                            await audioService.PlayAudioAsync(logItem, stoppingToken);
                        }
                    }

                    Log.Information("Worker running at: {time}", DateTimeOffset.Now);
                    await Task.Delay(1000, stoppingToken);
                }
            }
        }
    }
}
