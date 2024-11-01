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

        private async Task OnGridButtonClick(DayOfWeek day, int hour)
        {
            if (selectedStationId.HasValue && selectedMusicPatternId.HasValue)
            {
                // Update the database with the selectedStationId and selectedMusicPatternId for the specific day and hour
                var musicGridItem = new MusicGridItem
                {
                    Hour = $"{hour}:00",
                    Sunday = day == DayOfWeek.Sunday ? selectedMusicPatternId.ToString() : string.Empty,
                    Monday = day == DayOfWeek.Monday ? selectedMusicPatternId.ToString() : string.Empty,
                    Tuesday = day == DayOfWeek.Tuesday ? selectedMusicPatternId.ToString() : string.Empty,
                    Wednesday = day == DayOfWeek.Wednesday ? selectedMusicPatternId.ToString() : string.Empty,
                    Thursday = day == DayOfWeek.Thursday ? selectedMusicPatternId.ToString() : string.Empty,
                    Friday = day == DayOfWeek.Friday ? selectedMusicPatternId.ToString() : string.Empty,
                    Saturday = day == DayOfWeek.Saturday ? selectedMusicPatternId.ToString() : string.Empty
                };

                DbContext.MusicGridItems.Add(musicGridItem);
                await DbContext.SaveChangesAsync();

                // Update the UI
                StateHasChanged();
            }
        }

        private string GetPatternNameForCell(int day, int hour)
        {
            var dayOfWeek = (DayOfWeek)day;
            var musicGridItem = musicGridItems.FirstOrDefault(mgi => mgi.Hour == $"{hour}:00");
            if (musicGridItem != null)
            {
                return dayOfWeek switch
                {
                    DayOfWeek.Sunday => musicPatterns.FirstOrDefault(mp => mp.PatternId.ToString() == musicGridItem.Sunday)?.Name ?? string.Empty,
                    DayOfWeek.Monday => musicPatterns.FirstOrDefault(mp => mp.PatternId.ToString() == musicGridItem.Monday)?.Name ?? string.Empty,
                    DayOfWeek.Tuesday => musicPatterns.FirstOrDefault(mp => mp.PatternId.ToString() == musicGridItem.Tuesday)?.Name ?? string.Empty,
                    DayOfWeek.Wednesday => musicPatterns.FirstOrDefault(mp => mp.PatternId.ToString() == musicGridItem.Wednesday)?.Name ?? string.Empty,
                    DayOfWeek.Thursday => musicPatterns.FirstOrDefault(mp => mp.PatternId.ToString() == musicGridItem.Thursday)?.Name ?? string.Empty,
                    DayOfWeek.Friday => musicPatterns.FirstOrDefault(mp => mp.PatternId.ToString() == musicGridItem.Friday)?.Name ?? string.Empty,
                    DayOfWeek.Saturday => musicPatterns.FirstOrDefault(mp => mp.PatternId.ToString() == musicGridItem.Saturday)?.Name ?? string.Empty,
                    _ => string.Empty
                };
            }
            return string.Empty;
        }
        private void OnCellClick(DayOfWeek day, int hour)
        {
            // Handle the cell click event here
            Logger.LogInformation($"Cell clicked: {day} at {hour}:00");
        }
    }
}
