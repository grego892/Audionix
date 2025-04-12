using SharedLibrary.Models;
using SharedLibrary.Models.MusicSchedule;
using SharedLibrary.Repositories;
using Audionix.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using SharedLibrary.Services;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using static MudBlazor.CategoryTypes;

namespace Audionix.Components.Pages.MusicSchedule
{
    public partial class LogBuilder
    {
        public DateTime? LogBuilderLogDate { get; set; } = null;
        [Inject] private AppStateService? AppStateService { get; set; }
        [Inject] private LogService? LogService { get; set; }
        [Inject] private IStationRepository? StationRepository { get; set; }
        [Inject] private IMusicPatternRepository? MusicPatternRepository { get; set; }
        [Inject] private ISongCategoryRepository? SongCategoryRepository { get; set; }
        [Inject] private IAudioMetadataRepository? AudioMetadataRepository { get; set; }
        [Inject] private IProgramLogRepository? ProgramLogRepository { get; set; }
        [Inject] ISnackbar? Snackbar { get; set; }


        private MudDatePicker _logBuilderPicker;
        private List<int> musicPatterns = new();
        private List<Category> songCategoryList = new();
        private List<AudioMetadata> scheduledSongs = new();
        private List<ProgramLogItem> log = new();
        private Dictionary<string, int> songCategoryRotationIndex = new();
        private List<ProgramLogItem> newDaysLog = new();


        private async Task SchedulePressed()
        {
            if (LogBuilderLogDate.HasValue && StationRepository != null && AppStateService?.station != null)
            {
                var stationId = AppStateService.station.StationId;
                Snackbar.Add($"Scheduling begin for: {AppStateService.station.CallLetters}", Severity.Normal);

                var dayOfWeek = LogBuilderLogDate.Value.DayOfWeek;
                Snackbar.Add($"Scheduling using grid for {dayOfWeek}", Severity.Normal);

                musicPatterns = await MusicPatternRepository.GetMusicPatternsForDayAsync(stationId, dayOfWeek);
                Snackbar.Add($"MusicPatterns {musicPatterns.Count}", Severity.Normal);

                songCategoryList = await SongCategoryRepository.GetSongCategoriesForPatternsAsync(musicPatterns);
                Snackbar.Add($"songCategoryList {songCategoryList.Count}", Severity.Normal);

                scheduledSongs = await AudioMetadataRepository.GetScheduledSongsAsync(songCategoryList, songCategoryRotationIndex);
                Snackbar.Add($"scheduledSongs {scheduledSongs.Count}", Severity.Normal);

                newDaysLog = CreateProgramLogItems(scheduledSongs);

                if (newDaysLog.Count == 0)
                {
                    Snackbar?.Add("No songs scheduled for this day.  Check clock grid.", Severity.Warning);
                    return;
                }
                else
                {
                    Snackbar.Add($"newDaysLog {newDaysLog.Count}", Severity.Normal);
                }


                await ProgramLogRepository.AddNewDayLogToDbLogAsync(newDaysLog);
                Snackbar.Add($"Added new days log to Database.", Severity.Normal);

                int removeOlderDays = 2;
                await ProgramLogRepository.RemoveOlderDaysFromDbLogAsync(removeOlderDays);
                Snackbar.Add($"Removed logs older than {removeOlderDays} days before now.", Severity.Normal);



                await LogBuilderPickerOk();
            }

            LogService.Test();
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
            int logOrderId = 1;
            TimeOnly currentTime = new TimeOnly(0, 0); // Start at midnight

            foreach (var song in scheduledSongs)
            {
                var newLogItem = new ProgramLogItem
                {
                    LogOrderID = logOrderId++,
                    Title = song.Title,
                    Artist = song.Artist,
                    Date = DateOnly.FromDateTime(LogBuilderLogDate ?? DateTime.Now),
                    Name = song.Filename,
                    Length = song.Duration,
                    Intro = song.Intro,
                    Segue = song.Segue,
                    From = "Music",
                    StationId = AppStateService.station.StationId,
                    Status = StatusType.notPlayed,
                    TimeScheduled = currentTime,
                    Cue = "AutoStart",
                    SoundCodeId = song.SoundCodeId,
                };

                newDaysLog.Add(newLogItem);

                // Add the length of the current song to the current time for the next song
                currentTime = currentTime.Add(song.Duration);
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
