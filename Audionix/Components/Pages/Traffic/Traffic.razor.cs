using Audionix.Components.Pages.MusicSchedule;
using Audionix.Models;
using Microsoft.EntityFrameworkCore;

namespace Audionix.Components.Pages.Traffic;
public partial class Traffic
{
    DateTime? trafficDate = DateTime.Now.Date.AddDays(1);
    private List<Station> stations = new();
    private Guid selectedStationId;

    protected override async Task OnInitializedAsync()
    {
        stations = await DbContext.Stations.ToListAsync();
    }

    private async Task OnStationChanged(Guid stationId)
    {
        selectedStationId = stationId;
    }
}
