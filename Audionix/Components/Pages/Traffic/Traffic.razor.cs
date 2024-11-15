using Audionix.Components.Pages.MusicSchedule;
using Audionix.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components;
using Audionix.Shared.Models;
using Audionix.Shared.Data;

namespace Audionix.Components.Pages.Traffic;
public partial class Traffic
{
    [Inject] private AppStateService appStateService { get; set; }
    [Inject] private ApplicationDbContext DbContext { get; set; }

    DateTime? trafficDate = DateTime.Now.Date.AddDays(1);
    private List<Station> stations = new();

    //protected override async Task OnInitializedAsync()
    //{
    //    //stations = await DbContext.Stations.ToListAsync();
    //}

}
