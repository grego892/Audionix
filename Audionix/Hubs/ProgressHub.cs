using Microsoft.AspNetCore.SignalR;

namespace Audionix.Hubs
{
    public class ProgressHub : Hub
    {
        public async Task UpdateProgress(int progress)
        {
            await Clients.All.SendAsync("ReceiveProgress", progress);
        }
    }
}
