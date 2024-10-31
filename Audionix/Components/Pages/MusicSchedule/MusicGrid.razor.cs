using Audionix.Models;
using Audionix.Models.MusicSchedule;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using static MudBlazor.CategoryTypes;

namespace Audionix.Components.Pages.MusicSchedule
{
    public partial class MusicGrid
    {
        private List<Station> stations = new();
        private Guid? selectedStationId;
        private List<MusicGridItem> musicGridItems = new();
        private List<MusicPattern> musicPatterns = new();
        private Guid? selectedMusicPatternId;

        protected override async Task OnInitializedAsync()
        {
            await LoadDataAsync();
            InitializeGridData();
        }

        private async Task LoadDataAsync()
        {
            stations = await DbContext.Stations.ToListAsync();
            selectedStationId = null;
        }

        private void InitializeGridData()
        {
            for (int i = 0; i < 24; i++)
            {
                musicGridItems.Add(new MusicGridItem
                {
                    Hour = $"{i}:00",
                    Sunday = string.Empty,
                    Monday = string.Empty,
                    Tuesday = string.Empty,
                    Wednesday = string.Empty,
                    Thursday = string.Empty,
                    Friday = string.Empty,
                    Saturday = string.Empty
                });
            }
        }

        private async Task OnStationChanged(Guid? stationId)
        {
            selectedStationId = stationId;
            await LoadMusicPatterns();
            await FilterPatterns();
            //FilterCategories();
        }

        private async Task LoadMusicPatterns()
        {
            if (selectedStationId.HasValue)
            {
                musicPatterns = await DbContext.MusicPatterns
                    .Where(mp => mp.StationId == selectedStationId.Value)
                    .ToListAsync();
            }
            else
            {
                musicPatterns.Clear();
            }
            selectedMusicPatternId = null;
        }

        private async Task OnMusicPatternChanged(Guid? patternId)
        {
            selectedMusicPatternId = patternId;
            // Implement logic to handle music pattern change if needed
        }

        private async Task FilterPatterns()
        {
            // Implement pattern filtering logic here
        }

        private async Task AddMusicPattern()
        {
            // Implement add music pattern logic here
        }

        private async Task RemoveMusicPattern()
        {
            // Implement remove music pattern logic here
        }
    }
}
