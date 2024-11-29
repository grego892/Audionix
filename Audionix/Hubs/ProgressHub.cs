using Microsoft.AspNetCore.SignalR;

namespace Audionix.Hubs
{
    public class ProgressHub : Hub
    {
        public async Task UpdateProgress(int logOrderId, double currentTime, double totalTime)
        {
            await Clients.All.SendAsync("ReceiveProgress", logOrderId, currentTime, totalTime);
        }
    }
}
