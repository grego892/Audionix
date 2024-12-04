using SharedLibrary.Models;
using Microsoft.AspNetCore.SignalR;
using Serilog;
using static ATL.Logging.Log;

namespace Audionix.Hubs
{
    public class ProgressHub : Hub
    {
        public async Task UpdateProgress(int logOrderId, double currentTime, double totalTime)
        {
            await Clients.All.SendAsync("ReceiveProgress", logOrderId, currentTime, totalTime);
            //Log.Debug("Received progress update: LogOrderID: {LogOrderID}, CurrentTime: {CurrentTime}, TotalTime: {TotalTime}", logOrderId, currentTime, totalTime);
        }

        public async Task PlayNextAudio(Guid stationId)
        {
            await Clients.All.SendAsync("PlayNextAudio", stationId);
        }

        public async Task StopAudio(Guid stationId)
        {
            await Clients.All.SendAsync("StopAudio", stationId);
        }

        public async Task UpdateLogItemState(ProgramLogItem logItem)
        {
            await Clients.All.SendAsync("UpdateLogItemState", logItem);
        }
    }
}
