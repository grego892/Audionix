using AudionixAudioServer.Data;
using AudionixAudioServer.Models;
using AudionixAudioServer.Models.MusicSchedule;
using Microsoft.EntityFrameworkCore;
using AudionixAudioServer.Repositories;
using Microsoft.EntityFrameworkCore.Internal;
using Serilog;

namespace AudionixAudioServer.Repositories
{
    public class StationRepository : IStationRepository
    {
        private readonly ApplicationDbContext _context;

        public StationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Station>> GetStationsAsync()
        {
            return await _context.Stations.ToListAsync();
        }

        public async Task<Station?> GetStationByIdAsync(Guid stationId)
        {
            return await _context.Stations.AsNoTracking().FirstOrDefaultAsync(s => s.StationId == stationId);
        }

        public async Task AddStationAsync(Station station)
        {
            _context.Stations.Add(station);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateStationAsync(Station station)
        {
            _context.Stations.Update(station);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteStationAsync(Guid stationId)
        {
            var station = await _context.Stations.FindAsync(stationId);
            if (station != null)
            {
                _context.Stations.Remove(station);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Category>> GetCategoriesAsync(Guid stationId)
        {
            return await _context.Categories.Where(c => c.StationId == stationId).ToListAsync();
        }

        public async Task AddCategoryAsync(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCategoryAsync(Guid categoryId)
        {
            var category = await _context.Categories.FindAsync(categoryId);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Guid>> GetMusicPatternsForDayAsync(Guid stationId, DayOfWeek day)
        {
            var musicGridItems = await _context.MusicGridItems
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

        public async Task<List<Category>> GetCategoriesForPatternsAsync(List<Guid> musicPatterns)
        {
            var categories = new List<Category>();

            foreach (var patternId in musicPatterns)
            {
                var patternCategories = await _context.PatternCategories
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

        public async Task<List<AudioMetadata>> GetScheduledSongsAsync(List<Category> categories, Dictionary<string, int> categoryRotationIndex)
        {
            var scheduledSongs = new List<AudioMetadata>();

            foreach (var category in categories)
            {
                var audioFiles = await _context.AudioFiles
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

        public async Task AddNewDayLogToDbLogAsync(List<ProgramLogItem> newDaysLog)
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

        public async Task<List<MusicGridItem>> GetMusicGridItemsAsync(Guid stationId)
        {
            return await _context.MusicGridItems.Where(mgi => mgi.StationId == stationId).ToListAsync();
        }

        public async Task AddMusicGridItemAsync(MusicGridItem item)
        {
            _context.MusicGridItems.Add(item);
            await _context.SaveChangesAsync();
        }

        public async Task<List<MusicPattern>> GetMusicPatternsAsync(Guid stationId)
        {
            return await _context.MusicPatterns.Where(mp => mp.StationId == stationId).ToListAsync();
        }

        public async Task<List<AudioMetadata>> GetAudioFilesAsync()
        {
            return await _context.AudioFiles.AsNoTracking().ToListAsync();
        }

        public async Task<AudioMetadata?> GetAudioFileByIdAsync(int id)
        {
            return await _context.AudioFiles.AsNoTracking().FirstOrDefaultAsync(am => am.Id == id);
        }

        public async Task AddAudioFileAsync(AudioMetadata audioMetadata)
        {
            _context.AudioFiles.Add(audioMetadata);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAudioFileAsync(AudioMetadata audioMetadata)
        {
            _context.AudioFiles.Remove(audioMetadata);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Folder>> GetFoldersForStationAsync(Guid stationId)
        {
            return await _context.Folders.Where(f => f.StationId == stationId).ToListAsync();
        }

        public async Task<Folder?> GetFolderByIdAsync(int id)
        {
            return await _context.Folders.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task AddFolderAsync(Folder folder)
        {
            _context.Folders.Add(folder);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteFolderAsync(Folder folder)
        {
            _context.Folders.Remove(folder);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAudioMetadataAsync(AudioMetadata audioMetadata)
        {
            _context.AudioFiles.Update(audioMetadata);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateMusicGridItemAsync(MusicGridItem musicGridItem)
        {
            _context.MusicGridItems.Update(musicGridItem);
            await _context.SaveChangesAsync();
        }
        public async Task<AudioMetadata?> GetAudioFileByFilenameAsync(string filename)
        {
            return await _context.AudioFiles.AsNoTracking().FirstOrDefaultAsync(am => am.Filename == filename);
        }
        public async Task<AppSettings> GetAppSettingsDataPathAsync()
        {
            return await _context.AppSettings.FirstOrDefaultAsync();
        }
    }
}
