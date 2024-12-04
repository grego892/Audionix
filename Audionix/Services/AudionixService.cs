using SharedLibrary.Models;
using Serilog;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace Audionix.Services
{
    public class AudionixService : IHostedService
    {
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
