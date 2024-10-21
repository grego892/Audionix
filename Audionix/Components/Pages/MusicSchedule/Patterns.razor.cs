using Audionix.Data.StationLog;
using Audionix.Models;
using Audionix.Models.MusicSchedule;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

namespace Audionix.Components.Pages.MusicSchedule
{
    public partial class Patterns
    {
        private List<Category> categories = new();
        private List<Category> filteredCategories = new();
        private List<Station> stations = new();
        private Guid selectedStationId;
        protected override async Task OnInitializedAsync()
        {
            stations = await DbContext.Stations.ToListAsync();
            categories = await DbContext.Categories.ToListAsync();
            if (stations.Any())
            {
                selectedStationId = stations.First().StationId;
                FilterCategories();
            }
        }

        private void FilterCategories()
        {
            filteredCategories = categories.Where(c => c.StationId == selectedStationId).ToList();
        }
        private async Task OnStationChanged(Guid stationId)
        {
            selectedStationId = stationId;
            FilterCategories();
        }

        ////////////////////////////
        ///

    }
}
