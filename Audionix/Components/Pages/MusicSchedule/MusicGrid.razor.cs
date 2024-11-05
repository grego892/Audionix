using Audionix.Data.Migrations;
using Audionix.Models;
using Audionix.Models.MusicSchedule;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;


namespace Audionix.Components.Pages.MusicSchedule
{
    public partial class MusicGrid
    {
        private List<MusicGridItem> musicGridItems = new();
        private List<MusicPattern> musicPatterns = new();
        private Guid? selectedMusicPatternId;
        [Inject] private ApplicationDbContext? DbContext { get; set; }
        [Inject] private AppStateService? AppStateService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await LoadMusicPatterns();
            await InitializeGridData();
            AppStateService.OnStationChanged += HandleStationChanged;
        }

        private async void HandleStationChanged(object? sender, EventArgs e)
        {
            musicGridItems.Clear();
            musicPatterns.Clear();
            await LoadMusicPatterns();
            await InitializeGridData();
            StateHasChanged();
        }

        private async Task LoadDataAsync()
        {
            musicPatterns = await DbContext.MusicPatterns.ToListAsync();
        }

        private async Task InitializeGridData()
        {
            if (AppStateService.station != null)
            {
                var stationId = AppStateService.station.StationId;
                musicGridItems = await DbContext.MusicGridItems
                    .Where(mgi => mgi.StationId == stationId)
                    .ToListAsync();

                if (musicGridItems.Count == 0)
                {
                    for (int i = 0; i < 24; i++)
                    {
                        var newItem = new MusicGridItem
                        {
                            Hour = $"{i}:00",
                            StationId = stationId,
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
                    musicGridItems = await DbContext.MusicGridItems
                        .Where(mgi => mgi.StationId == stationId)
                        .ToListAsync();
                }
            }
        }

        private async Task LoadMusicPatterns()
        {
            if (AppStateService.station != null)
            {
                var stationId = AppStateService.station.StationId;
                musicPatterns = await DbContext.MusicPatterns
                    .Where(mp => mp.StationId == stationId)
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

        private async Task OnGridButtonClick(DayOfWeek day, int hour)
        {
            if (AppStateService.station != null && selectedMusicPatternId.HasValue)
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
            if (AppStateService.station != null && selectedMusicPatternId.HasValue)
            {
                var stationId = AppStateService.station.StationId;
                var musicGridItem = await DbContext.MusicGridItems
                    .FirstOrDefaultAsync(mgi => mgi.Hour == $"{hour}:00" && mgi.StationId == stationId);

                if (musicGridItem != null)
                {
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
                    StateHasChanged();
                }
            }
        }
    }
}
