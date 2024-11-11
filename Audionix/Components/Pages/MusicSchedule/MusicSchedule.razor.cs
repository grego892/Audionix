using Audionix.Models.MusicSchedule;
using Audionix.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Audionix.Components.Pages.MusicSchedule
{
    public partial class MusicSchedule
    {
        public DateTime? MusicLogDate { get; set; } = DateTime.Now.Date.AddDays(1);
        [Inject] private ApplicationDbContext? DbContext { get; set; }
        [Inject] private AppStateService? AppStateService { get; set; }

        private List<Guid> musicPatterns = new();
        private List<Category> categoryList = new();
        private List<AudioMetadata> scheduledSongs = new();
        private Dictionary<string, int> categoryRotationIndex = new();
        private List<ProgramLogItem> newDaysLog = new();

        private async Task SchedulePressed()
        {
            if (MusicLogDate.HasValue)
            {
                var dayOfWeek = MusicLogDate.Value.DayOfWeek;
                musicPatterns = await GetMusicPatternsForDayAsync(dayOfWeek);
                categoryList = await GetCategoriesForPatternsAsync(musicPatterns);
                scheduledSongs = await GetScheduledSongsAsync(categoryList);
                newDaysLog = await CreateProgramLogItemsAsync(scheduledSongs);
                await AddNewDayLogToDbLog(newDaysLog);
            }
        }

        private async Task<List<Guid>> GetMusicPatternsForDayAsync(DayOfWeek day)
        {
            if (DbContext == null)
            {
                throw new InvalidOperationException("DbContext is not initialized.");
            }

            if (AppStateService == null || AppStateService.station == null)
            {
                throw new InvalidOperationException("AppStateService or station is not initialized.");
            }

            var musicGridItems = await DbContext.MusicGridItems
                .Where(item => item.StationId == AppStateService.station.StationId)
                .ToListAsync();

            var patternIds = musicGridItems.Select(item => day switch
            {
                DayOfWeek.Sunday => item.SundayPatternId,
                DayOfWeek.Monday => item.MondayPatternId,
                DayOfWeek.Tuesday => item.TuesdayPatternId,
                DayOfWeek.Wednesday => item.WednesdayPatternId,
                DayOfWeek.Thursday => item.ThursdayPatternId,
                DayOfWeek.Friday => item.FridayPatternId,
                DayOfWeek.Saturday => item.SaturdayPatternId,
                _ => null
            }).Where(id => id.HasValue).Select(id => id!.Value).ToList();

            return patternIds;
        }

        private async Task<List<Category>> GetCategoriesForPatternsAsync(List<Guid> musicPatterns)
        {
            if (DbContext == null)
            {
                throw new InvalidOperationException("DbContext is not initialized.");
            }

            var categories = new List<Category>();

            foreach (var patternId in musicPatterns)
            {
                var patternCategories = await DbContext.PatternCategories
                    .Where(pc => pc.MusicPatternId == patternId)
                    .Include(pc => pc.Category)
                    .ToListAsync();

                foreach (var patternCategory in patternCategories)
                {
                    if (patternCategory.Category != null)
                    {
                        categories.Add(patternCategory.Category);
                    }
                }
            }

            return categories;
        }

        private async Task<List<AudioMetadata>> GetScheduledSongsAsync(List<Category> categories)
        {
            if (DbContext == null)
            {
                throw new InvalidOperationException("DbContext is not initialized.");
            }

            var scheduledSongs = new List<AudioMetadata>();

            foreach (var category in categories)
            {
                Log.Information($"--- MusicSchedule - GetScheduledSongsAsync() -- Going through categories - category: {category.CategoryName}");
                var audioFiles = await DbContext.AudioFiles
                    .Where(af => af.SelectedCategory == category.CategoryName)
                    .ToListAsync();

                if (audioFiles.Any())
                {
                    // Get the last used index for this category
                    if (!categoryRotationIndex.TryGetValue(category.CategoryName, out int lastIndex))
                    {
                        lastIndex = 0;
                    }

                    // Get the next song in the rotation
                    var nextIndex = (lastIndex + 1) % audioFiles.Count;
                    var rotatedSong = audioFiles[nextIndex];

                    // Update the last used index for this category
                    categoryRotationIndex[category.CategoryName] = nextIndex;

                    // Add the rotated song to the scheduled songs
                    scheduledSongs.Add(rotatedSong);
                }
            }

            return scheduledSongs;
        }

        private async Task<List<ProgramLogItem>> CreateProgramLogItemsAsync(List<AudioMetadata> scheduledSongs)
        {
            if (DbContext == null)
            {
                throw new InvalidOperationException("DbContext is not initialized.");
            }

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
                    Date = DateOnly.FromDateTime(MusicLogDate.Value),
                    //TimeScheduled = new TimeOnly(0, 0),
                    Name = song.Filename,
                    Length = song.Duration.ToString(),
                    Segue = "0",
                    StationId = AppStateService.station.StationId
                };

                newDaysLog.Add(newLogItem);
            }

            return newDaysLog;
        }

        //private async Task AddNewDayLogToDbLog(List<ProgramLogItem> newDaysLog)
        //{
        //    if (DbContext == null)
        //    {
        //        throw new InvalidOperationException("DbContext is not initialized.");
        //    }

        //    DbContext.Log.AddRange(newDaysLog);
        //    await DbContext.SaveChangesAsync();
        //}

        private async Task AddNewDayLogToDbLog(List<ProgramLogItem> newDaysLog)
        {
            if (DbContext == null)
            {
                throw new InvalidOperationException("DbContext is not initialized.");
            }

            if (newDaysLog == null || !newDaysLog.Any())
            {
                throw new ArgumentException("newDaysLog is null or empty.");
            }

            var newLogDate = newDaysLog.First().Date;

            // Retrieve the maximum LogOrderID for the previous date in the log
            int maxLogOrderIDForPreviousDate = await DbContext.Log
                .Where(log => log.Date < newLogDate)
                .MaxAsync(log => (int?)log.LogOrderID) ?? 0;

            // Assign LogOrderID to the new log items starting from the retrieved maximum LogOrderID
            int currentLogOrderID = maxLogOrderIDForPreviousDate;
            foreach (var logItem in newDaysLog)
            {
                logItem.LogOrderID = ++currentLogOrderID;
            }

            // Insert the new log items into the database
            await DbContext.Log.AddRangeAsync(newDaysLog);
            await DbContext.SaveChangesAsync();

            // Renumber all log items for dates after the new log items' date
            var logsToRenumber = await DbContext.Log
                .Where(log => log.Date > newLogDate)
                .OrderBy(log => log.Date)
                .ThenBy(log => log.LogOrderID)
                .ToListAsync();

            currentLogOrderID = newDaysLog.Max(log => log.LogOrderID);
            foreach (var logItem in logsToRenumber)
            {
                logItem.LogOrderID = ++currentLogOrderID;
            }

            DbContext.Log.UpdateRange(logsToRenumber);
            await DbContext.SaveChangesAsync();
        }

    }
}
