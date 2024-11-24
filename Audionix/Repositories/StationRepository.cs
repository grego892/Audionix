using Audionix.Data;
using Audionix.Models;
using Audionix.Models.MusicSchedule;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Threading;

namespace Audionix.Repositories
{
    public class StationRepository : IStationRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public StationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Station>> GetStationsAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                return await _context.Stations.ToListAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<Station?> GetStationByIdAsync(Guid stationId)
        {
            await _semaphore.WaitAsync();
            try
            {
                return await _context.Stations.FindAsync(stationId);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task AddStationAsync(Station station)
        {
            await _semaphore.WaitAsync();
            try
            {
                await _context.Stations.AddAsync(station);
                await _context.SaveChangesAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task UpdateStationAsync(Station station)
        {
            await _semaphore.WaitAsync();
            try
            {
                _context.Stations.Update(station);
                await _context.SaveChangesAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task DeleteStationAsync(Guid stationId)
        {
            await _semaphore.WaitAsync();
            try
            {
                var station = await _context.Stations.FindAsync(stationId);
                if (station != null)
                {
                    _context.Stations.Remove(station);
                    await _context.SaveChangesAsync();
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<List<Category>> GetCategoriesAsync(Guid stationId)
        {
            await _semaphore.WaitAsync();
            try
            {
                return await _context.Categories.Where(c => c.StationId == stationId).ToListAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task AddCategoryAsync(Category category)
        {
            await _semaphore.WaitAsync();
            try
            {
                await _context.Categories.AddAsync(category);
                await _context.SaveChangesAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task DeleteCategoryAsync(Guid categoryId)
        {
            await _semaphore.WaitAsync();
            try
            {
                var category = await _context.Categories.FindAsync(categoryId);
                if (category != null)
                {
                    _context.Categories.Remove(category);
                    await _context.SaveChangesAsync();
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<List<Guid>> GetMusicPatternsForDayAsync(Guid stationId, DayOfWeek day)
        {
            await _semaphore.WaitAsync();
            try
            {
                var musicGridItems = await _context.MusicGridItems
                    .AsNoTracking()
                    .Where(item => item.StationId == stationId)
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
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<List<Category>> GetCategoriesForPatternsAsync(List<Guid> musicPatterns)
        {
            await _semaphore.WaitAsync();
            try
            {
                var categories = new List<Category>();

                foreach (var patternId in musicPatterns)
                {
                    var patternCategories = await _context.PatternCategories
                        .AsNoTracking()
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
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<List<AudioMetadata>> GetScheduledSongsAsync(List<Category> categories, Dictionary<string, int> categoryRotationIndex)
        {
            await _semaphore.WaitAsync();
            try
            {
                var scheduledSongs = new List<AudioMetadata>();

                foreach (var category in categories)
                {
                    var audioFiles = await _context.AudioFiles
                        .AsNoTracking()
                        .Where(af => af.SelectedCategory == category.CategoryName)
                        .ToListAsync();

                    if (audioFiles.Any())
                    {
                        // Get the last used index for this category
                        if (!categoryRotationIndex.TryGetValue(category.CategoryName ?? string.Empty, out int lastIndex))
                        {
                            lastIndex = 0;
                        }

                        // Get the next song in the rotation
                        var nextIndex = (lastIndex + 1) % audioFiles.Count;
                        var rotatedSong = audioFiles[nextIndex];

                        // Update the last used index for this category
                        categoryRotationIndex[category.CategoryName ?? string.Empty] = nextIndex;

                        // Add the rotated song to the scheduled songs
                        scheduledSongs.Add(rotatedSong);
                    }
                }

                return scheduledSongs;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task AddNewDayLogToDbLogAsync(List<ProgramLogItem> newDaysLog)
        {
            await _semaphore.WaitAsync();
            try
            {
                if (newDaysLog != null && newDaysLog.Count > 0)
                {
                    if (newDaysLog == null || !newDaysLog.Any())
                    {
                        Log.Error("+++ AppDatabaseService - AddNewDayLogToDbLogAsync() -- NewDaysLog is null or empty.");
                    }

                    var newLogDate = newDaysLog.First().Date;

                    // Retrieve the maximum LogOrderID for the previous date in the log
                    int maxLogOrderIDForPreviousDate = await _context.Log
                        .AsNoTracking()
                        .Where(log => log.Date < newLogDate)
                        .MaxAsync(log => (int?)log.LogOrderID) ?? 0;

                    // Assign LogOrderID to the new log items starting from the retrieved maximum LogOrderID
                    int currentLogOrderID = maxLogOrderIDForPreviousDate;
                    foreach (var logItem in newDaysLog)
                    {
                        logItem.LogOrderID = ++currentLogOrderID;
                    }

                    // Insert the new log items into the database
                    await _context.Log.AddRangeAsync(newDaysLog);
                    await _context.SaveChangesAsync();

                    // Renumber all log items for dates after the new log items' date
                    var logsToRenumber = await _context.Log
                        .Where(log => log.Date > newLogDate)
                        .OrderBy(log => log.Date)
                        .ThenBy(log => log.LogOrderID)
                        .ToListAsync();

                    currentLogOrderID = newDaysLog.Max(log => log.LogOrderID);
                    foreach (var logItem in logsToRenumber)
                    {
                        logItem.LogOrderID = ++currentLogOrderID;
                    }

                    _context.Log.UpdateRange(logsToRenumber);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    Log.Error("+++ AppDatabaseService - AddNewDayLogToDbLogAsync() -- NewDaysLog is null or empty.");
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<List<MusicGridItem>> GetMusicGridItemsAsync(Guid stationId)
        {
            await _semaphore.WaitAsync();
            try
            {
                return await _context.MusicGridItems.Where(m => m.StationId == stationId).ToListAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task AddMusicGridItemAsync(MusicGridItem item)
        {
            await _semaphore.WaitAsync();
            try
            {
                await _context.MusicGridItems.AddAsync(item);
                await _context.SaveChangesAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<List<MusicPattern>> GetMusicPatternsAsync(Guid stationId)
        {
            await _semaphore.WaitAsync();
            try
            {
                return await _context.MusicPatterns.Where(m => m.StationId == stationId).ToListAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<List<AudioMetadata>> GetAudioFilesAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                return await _context.AudioFiles.ToListAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<AudioMetadata?> GetAudioFileByIdAsync(int id)
        {
            await _semaphore.WaitAsync();
            try
            {
                return await _context.AudioFiles.FindAsync(id);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task AddAudioFileAsync(AudioMetadata audioMetadata)
        {
            await _semaphore.WaitAsync();
            try
            {
                await _context.AudioFiles.AddAsync(audioMetadata);
                await _context.SaveChangesAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task DeleteAudioFileAsync(AudioMetadata audioMetadata)
        {
            await _semaphore.WaitAsync();
            try
            {
                _context.AudioFiles.Remove(audioMetadata);
                await _context.SaveChangesAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<List<Folder>> GetFoldersForStationAsync(Guid stationId)
        {
            await _semaphore.WaitAsync();
            try
            {
                return await _context.Folders.Where(f => f.StationId == stationId).ToListAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<Folder?> GetFolderByIdAsync(int id)
        {
            await _semaphore.WaitAsync();
            try
            {
                return await _context.Folders.FindAsync(id);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task AddFolderAsync(Folder folder)
        {
            await _semaphore.WaitAsync();
            try
            {
                await _context.Folders.AddAsync(folder);
                await _context.SaveChangesAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task DeleteFolderAsync(Folder folder)
        {
            await _semaphore.WaitAsync();
            try
            {
                _context.Folders.Remove(folder);
                await _context.SaveChangesAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task UpdateAudioMetadataAsync(AudioMetadata audioMetadata)
        {
            await _semaphore.WaitAsync();
            try
            {
                _context.AudioFiles.Update(audioMetadata);
                await _context.SaveChangesAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task UpdateMusicGridItemAsync(MusicGridItem musicGridItem)
        {
            await _semaphore.WaitAsync();
            try
            {
                _context.MusicGridItems.Update(musicGridItem);
                await _context.SaveChangesAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<AudioMetadata> GetAudioFileByFilenameAsync(string filename)
        {
            await _semaphore.WaitAsync();
            try
            {
                return await _context.AudioFiles.FirstOrDefaultAsync(a => a.Filename == filename);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<AppSettings?> GetAppSettingsAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                return await _context.AppSettings.FirstOrDefaultAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task SaveAppSettingsAsync(AppSettings appSettings)
        {
            await _semaphore.WaitAsync();
            try
            {
                _context.AppSettings.Update(appSettings);
                await _context.SaveChangesAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
