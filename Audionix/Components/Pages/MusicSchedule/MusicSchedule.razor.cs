using Audionix.Models;
using Audionix.Models.MusicSchedule;
using Audionix.Services;
using Microsoft.AspNetCore.Components;
using Serilog;

namespace Audionix.Components.Pages.MusicSchedule
{
    public partial class MusicSchedule
    {
        public DateTime? MusicLogDate { get; set; } = DateTime.Now.Date.AddDays(1);
        [Inject] private AppStateService? AppStateService { get; set; }
        [Inject] private AppDatabaseService? DatabaseService { get; set; }

        private List<Guid> musicPatterns = new();
        private List<Category> categoryList = new();
        private List<AudioMetadata> scheduledSongs = new();
        private Dictionary<string, int> categoryRotationIndex = new();
        private List<ProgramLogItem> newDaysLog = new();

        private async Task SchedulePressed()
        {
            if (MusicLogDate.HasValue && DatabaseService != null && AppStateService?.station != null)
            {
                var dayOfWeek = MusicLogDate.Value.DayOfWeek;
                var stationId = AppStateService.station.StationId;
                musicPatterns = await DatabaseService.GetMusicPatternsForDayAsync(stationId, dayOfWeek);
                categoryList = await DatabaseService.GetCategoriesForPatternsAsync(musicPatterns);
                scheduledSongs = await DatabaseService.GetScheduledSongsAsync(categoryList, categoryRotationIndex);
                newDaysLog = CreateProgramLogItems(scheduledSongs);
                await DatabaseService.AddNewDayLogToDbLogAsync(newDaysLog);
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
                    Date = DateOnly.FromDateTime(MusicLogDate ?? DateTime.Now),
                    Name = song.Filename,
                    Length = song.Duration.ToString(),
                    Segue = "0",
                    StationId = AppStateService.station.StationId
                };

                newDaysLog.Add(newLogItem);
            }

            return newDaysLog;
        }
    }
}
