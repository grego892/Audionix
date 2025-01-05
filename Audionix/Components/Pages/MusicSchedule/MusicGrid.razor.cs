using SharedLibrary.Models.MusicSchedule;
using SharedLibrary.Repositories;
using Audionix.Services;
using Microsoft.AspNetCore.Components;

namespace Audionix.Components.Pages.MusicSchedule
{
    public partial class MusicGrid
    {
        private List<MusicGridItem> musicGridItems = new();
        private List<MusicPattern> musicPatterns = new();
        private Guid? selectedMusicPatternId;
        [Inject] private AppStateService? AppStateService { get; set; }
        [Inject] private IStationRepository? StationRepository { get; set; }
        [Inject] private IMusicPatternRepository? MusicPatternRepository { get; set; }

        protected override async Task OnInitializedAsync()
        {
            if (AppStateService != null)
            {
                await LoadMusicPatterns();
                await InitializeGridData();
                AppStateService.OnStationChanged += HandleStationChanged;
            }
        }

        private async void HandleStationChanged(object? sender, EventArgs e)
        {
            musicGridItems.Clear();
            musicPatterns.Clear();
            await LoadMusicPatterns();
            await InitializeGridData();
            StateHasChanged();
        }

        private async Task InitializeGridData()
        {
            if (AppStateService?.station != null && StationRepository != null)
            {
                var stationId = AppStateService.station.StationId;
                musicGridItems = await MusicPatternRepository.GetMusicGridItemsAsync(stationId);

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
                        await MusicPatternRepository.AddMusicGridItemAsync(newItem);
                    }
                    musicGridItems = await MusicPatternRepository.GetMusicGridItemsAsync(stationId);
                }
            }
        }

        private async Task LoadMusicPatterns()
        {
            if (AppStateService?.station != null && StationRepository != null)
            {
                var stationId = AppStateService.station.StationId;
                musicPatterns = await MusicPatternRepository.GetMusicPatternsAsync(stationId);
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
            if (AppStateService?.station != null && selectedMusicPatternId.HasValue && StationRepository != null)
            {
                var stationId = AppStateService.station.StationId;
                var musicGridItem = musicGridItems.FirstOrDefault(mgi => mgi.Hour == $"{hour}:00" && mgi.StationId == stationId);

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

                    await MusicPatternRepository.UpdateMusicGridItemAsync(musicGridItem);
                    StateHasChanged();
                }
            }
        }
    }
}
