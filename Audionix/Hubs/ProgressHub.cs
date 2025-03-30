using SharedLibrary.Models;
using Microsoft.AspNetCore.SignalR;
using Serilog;
using static ATL.Logging.Log;

namespace Audionix.Hubs
{
    public class ProgressHub : Hub
    {
        public async Task UpdateProgress(int logOrderId, DateOnly logOrderDate, double currentTime, double totalTime)
        {
            await Clients.All.SendAsync("ReceiveProgress", logOrderId, logOrderDate, currentTime, totalTime);
        }

        public async Task PlayNextAudio(int stationId)
        {
            await Clients.All.SendAsync("PlayNextAudio", stationId);
        }

        public async Task StopAudio(int stationId)
        {
            await Clients.All.SendAsync("StopAudio", stationId);
        }

        public async Task UpdateLogItemState(ProgramLogItem logItem)
        {
            await Clients.All.SendAsync("UpdateLogItemState", logItem);
        }
        public async Task SendOffer(string offer)
        {
            await Clients.All.SendAsync("ReceiveOffer", offer);
        }

        public async Task SendAnswer(string answer)
        {
            await Clients.All.SendAsync("ReceiveAnswer", answer);
        }

        public async Task SendIceCandidate(string candidate)
        {
            await Clients.All.SendAsync("ReceiveIceCandidate", candidate);
        }
    }
}
