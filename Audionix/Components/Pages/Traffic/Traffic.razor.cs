using Audionix.Components.Pages.MusicSchedule;
using Audionix.Models;
using Audionix.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components;

namespace Audionix.Components.Pages.Traffic;
public partial class Traffic
{
    [Inject]
    private AppStateService appStateService { get; set; }

    [Inject]
    private ApplicationDbContext DbContext { get; set; }

    DateTime? trafficDate = DateTime.Now.Date.AddDays(1);
    private List<Station> stations = new();

    protected override async Task OnInitializedAsync()
    {
        stations = await DbContext.Stations.ToListAsync();
    }

    private async Task OnStationChanged(Guid stationId)
    {
        appStateService.station = stations.FirstOrDefault(s => s.StationId == stationId);
    }
}
