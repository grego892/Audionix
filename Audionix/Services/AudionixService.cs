using Audionix.Shared;
using Serilog;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using Audionix.Shared.Models;

namespace Audionix.Services
{
    public class AudionixService : IHostedService
    {
        private readonly AppSettings _appSettings;

        public AudionixService(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Log.Information("+++ AudionixService - StartAsync() -- Audionix Service starting");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Log.Information("+++ AudionixService - StopAsync() -- Audionix Service stopping");
            return Task.CompletedTask;
        }
    }
}
