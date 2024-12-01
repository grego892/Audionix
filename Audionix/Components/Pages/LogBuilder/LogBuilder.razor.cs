using Audionix.Models;
using Audionix.Models.MusicSchedule;
using Audionix.Repositories;
using Audionix.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Serilog;

namespace Audionix.Components.Pages.LogBuilder
{
    public partial class LogBuilder
    {
        public DateTime? LogBuilderLogDate { get; set; } = DateTime.Now.Date.AddDays(1);
        [Inject] private AppStateService? AppStateService { get; set; }
        [Inject] private IStationRepository? StationRepository { get; set; }
        [Inject] private IMusicPatternRepository? MusicPatternRepository { get; set; }
        [Inject] private ICategoryRepository? CategoryRepository { get; set; }
        [Inject] private IAudioMetadataRepository? AudioMetadataRepository { get; set; }
        [Inject] private IProgramLogRepository? ProgramLogRepository { get; set; }


        private MudDatePicker _logBuilderPicker;
        private List<Guid> musicPatterns = new();
        private List<Category> categoryList = new();
        private List<AudioMetadata> scheduledSongs = new();
        private List<ProgramLogItem> log = new();
        private Dictionary<string, int> categoryRotationIndex = new();
        private List<ProgramLogItem> newDaysLog = new();


        private async Task SchedulePressed()
        {
            if (LogBuilderLogDate.HasValue && StationRepository != null && AppStateService?.station != null)
            {
                var dayOfWeek = LogBuilderLogDate.Value.DayOfWeek;
                var stationId = AppStateService.station.StationId;
                musicPatterns = await MusicPatternRepository.GetMusicPatternsForDayAsync(stationId, dayOfWeek);
                categoryList = await CategoryRepository.GetCategoriesForPatternsAsync(musicPatterns);
                scheduledSongs = await AudioMetadataRepository.GetScheduledSongsAsync(categoryList, categoryRotationIndex);
                newDaysLog = CreateProgramLogItems(scheduledSongs);
                await ProgramLogRepository.AddNewDayLogToDbLogAsync(newDaysLog);
            }
        }

        private void LogBuilderPickerSetToday()
        {
            if (_logBuilderPicker != null)
            {
                LogBuilderLogDate = DateTime.Today;
            }
        }

        private void LogBuilderPickerSetTomorrow()
        {
            if (_logBuilderPicker != null)
            {
                LogBuilderLogDate = DateTime.Now.AddDays(1);
            }
        }

        private async Task LogBuilderPickerOk()
        {
            await _logBuilderPicker.CloseAsync();
            if (AppStateService?.station != null && LogBuilderLogDate.HasValue)
            {
                log = await ProgramLogRepository.GetProgramLogItemsAsync(AppStateService.station.StationId, DateOnly.FromDateTime(LogBuilderLogDate.Value));
            }
        }

        private List<ProgramLogItem> CreateProgramLogItems(List<AudioMetadata> scheduledSongs)
        {
            if (AppStateService == null || AppStateService.station == null)
            {
                throw new InvalidOperationException("AppStateService or station is not initialized.");
            }

            var newDaysLog = new List<ProgramLogItem>();

            foreach (var song in scheduledSongs)
            {
                var newLogItem = new ProgramLogItem
                {
                    Title = song.Title,
                    Artist = song.Artist,
                    Date = DateOnly.FromDateTime(LogBuilderLogDate ?? DateTime.Now),
                    Name = song.Filename,
                    Length = song.Duration.ToString(),
                    Intro = song.Intro,
                    Segue = song.Segue,
                    StationId = AppStateService.station.StationId,
                    States = StatesType.notPlayed,
                    AudioType = song.AudioType
                };

                newDaysLog.Add(newLogItem);
            }

            return newDaysLog;
        }
        private async Task LoadMusicFromFile()
        {
            await Task.CompletedTask;
        }
        private async Task LoadTrafficFromFile()
        {
            await Task.CompletedTask;
        }
    }
}
