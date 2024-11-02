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
            musicPatterns = await DbContext.MusicPatterns.ToListAsync();
        }

        private async Task InitializeGridData()
        {
            musicGridItems = await DbContext.MusicGridItems.ToListAsync();
            if (musicGridItems.Count == 0)
            {
                for (int i = 0; i < 24; i++)
                {
                    var newItem = new MusicGridItem
                    {
                        Hour = $"{i}:00",
                        Sunday = string.Empty,
                        Monday = string.Empty,
                        Tuesday = string.Empty,
                        Wednesday = string.Empty,
                        Thursday = string.Empty,
                        Friday = string.Empty,
                        Saturday = string.Empty
                    };
                    DbContext.MusicGridItems.Add(newItem);
                }
                await DbContext.SaveChangesAsync();
                musicGridItems = await DbContext.MusicGridItems.ToListAsync();
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

        private Task OnMusicPatternChanged(Guid? patternId)
        {
            selectedMusicPatternId = patternId;
            return Task.CompletedTask;
        }

        private Task FilterPatterns()
        {
            // Implement pattern filtering logic here
            return Task.CompletedTask;
        }

        private Task AddMusicPattern()
        {
            // Implement add music pattern logic here
            return Task.CompletedTask;
        }

        private Task RemoveMusicPattern()
        {
            // Implement remove music pattern logic here
            return Task.CompletedTask;
        }

        private async Task OnGridButtonClick(DayOfWeek day, int hour)
        {
            if (selectedStationId.HasValue && selectedMusicPatternId.HasValue)
            {
                var musicGridItem = new MusicGridItem
                {
                    Hour = $"{hour}:00",
                    Sunday = day == DayOfWeek.Sunday ? selectedMusicPatternId?.ToString() ?? string.Empty : string.Empty,
                    Monday = day == DayOfWeek.Monday ? selectedMusicPatternId?.ToString() ?? string.Empty : string.Empty,
                    Tuesday = day == DayOfWeek.Tuesday ? selectedMusicPatternId?.ToString() ?? string.Empty : string.Empty,
                    Wednesday = day == DayOfWeek.Wednesday ? selectedMusicPatternId?.ToString() ?? string.Empty : string.Empty,
                    Thursday = day == DayOfWeek.Thursday ? selectedMusicPatternId?.ToString() ?? string.Empty : string.Empty,
                    Friday = day == DayOfWeek.Friday ? selectedMusicPatternId?.ToString() ?? string.Empty : string.Empty,
                    Saturday = day == DayOfWeek.Saturday ? selectedMusicPatternId?.ToString() ?? string.Empty : string.Empty
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
                    DayOfWeek.Sunday => musicPatterns.FirstOrDefault(mp => mp.PatternId == musicGridItem.SundayPatternId)?.Name ?? string.Empty,
                    DayOfWeek.Monday => musicPatterns.FirstOrDefault(mp => mp.PatternId == musicGridItem.MondayPatternId)?.Name ?? string.Empty,
                    DayOfWeek.Tuesday => musicPatterns.FirstOrDefault(mp => mp.PatternId == musicGridItem.TuesdayPatternId)?.Name ?? string.Empty,
                    DayOfWeek.Wednesday => musicPatterns.FirstOrDefault(mp => mp.PatternId == musicGridItem.WednesdayPatternId)?.Name ?? string.Empty,
                    DayOfWeek.Thursday => musicPatterns.FirstOrDefault(mp => mp.PatternId == musicGridItem.ThursdayPatternId)?.Name ?? string.Empty,
                    DayOfWeek.Friday => musicPatterns.FirstOrDefault(mp => mp.PatternId == musicGridItem.FridayPatternId)?.Name ?? string.Empty,
                    DayOfWeek.Saturday => musicPatterns.FirstOrDefault(mp => mp.PatternId == musicGridItem.SaturdayPatternId)?.Name ?? string.Empty,
                    _ => string.Empty
                };
            }
            return string.Empty;
        }

        private async Task OnCellClick(DayOfWeek day, int hour)
        {
            if (selectedStationId.HasValue && selectedMusicPatternId.HasValue)
            {
                // Find the existing MusicGridItem for the specified hour
                var musicGridItem = await DbContext.MusicGridItems
                    .FirstOrDefaultAsync(mgi => mgi.Hour == $"{hour}:00");

                if (musicGridItem != null)
                {
                    // Update the appropriate day with the selected PatternId
                    switch (day)
                    {
                        case DayOfWeek.Sunday:
                            musicGridItem.SundayPatternId = selectedMusicPatternId;
                            break;
                        case DayOfWeek.Monday:
                            musicGridItem.MondayPatternId = selectedMusicPatternId;
                            break;
                        case DayOfWeek.Tuesday:
                            musicGridItem.TuesdayPatternId = selectedMusicPatternId;
                            break;
                        case DayOfWeek.Wednesday:
                            musicGridItem.WednesdayPatternId = selectedMusicPatternId;
                            break;
                        case DayOfWeek.Thursday:
                            musicGridItem.ThursdayPatternId = selectedMusicPatternId;
                            break;
                        case DayOfWeek.Friday:
                            musicGridItem.FridayPatternId = selectedMusicPatternId;
                            break;
                        case DayOfWeek.Saturday:
                            musicGridItem.SaturdayPatternId = selectedMusicPatternId;
                            break;
                    }

                    await DbContext.SaveChangesAsync();

                    // Update the UI
                    StateHasChanged();
                }
            }
        }
    }
}