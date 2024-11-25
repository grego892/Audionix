using Audionix.Data;
using Audionix.Models;
using Audionix.Models.MusicSchedule;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Audionix.Repositories
{
    public class StationRepository : IStationRepository
    {
        private readonly IDbContextFactory _dbContextFactory;

        public StationRepository(IDbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<List<Station>> GetStationsAsync()
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.Stations.ToListAsync();
        }

        public async Task<Station?> GetStationByIdAsync(Guid stationId)
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.Stations.FindAsync(stationId);
        }

        public async Task AddStationAsync(Station station)
        {
            using var context = _dbContextFactory.CreateDbContext();
            await context.Stations.AddAsync(station);
            await context.SaveChangesAsync();
        }

        public async Task UpdateStationAsync(Station station)
        {
            using var context = _dbContextFactory.CreateDbContext();
            context.Stations.Update(station);
            await context.SaveChangesAsync();
        }

        public async Task DeleteStationAsync(Guid stationId)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var station = await context.Stations.FindAsync(stationId);
            if (station != null)
            {
                context.Stations.Remove(station);
                await context.SaveChangesAsync();
            }
        }

        public async Task<List<Category>> GetCategoriesAsync(Guid stationId)
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.Categories.Where(c => c.StationId == stationId).ToListAsync();
        }

        public async Task AddCategoryAsync(Category category)
        {
            using var context = _dbContextFactory.CreateDbContext();
            await context.Categories.AddAsync(category);
            await context.SaveChangesAsync();
        }

        public async Task DeleteCategoryAsync(Guid categoryId)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var category = await context.Categories.FindAsync(categoryId);
            if (category != null)
            {
                context.Categories.Remove(category);
                await context.SaveChangesAsync();
            }
        }

        public async Task<List<Guid>> GetMusicPatternsForDayAsync(Guid stationId, DayOfWeek day)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var musicGridItems = await context.MusicGridItems
                .AsNoTracking()
                .Where(item => item.StationId == stationId)
                .ToListAsync();

            return musicGridItems
                .Select(item => day switch
                {
                    DayOfWeek.Sunday => item.SundayPatternId,
                    DayOfWeek.Monday => item.MondayPatternId,
                    DayOfWeek.Tuesday => item.TuesdayPatternId,
                    DayOfWeek.Wednesday => item.WednesdayPatternId,
                    DayOfWeek.Thursday => item.ThursdayPatternId,
                    DayOfWeek.Friday => item.FridayPatternId,
                    DayOfWeek.Saturday => item.SaturdayPatternId,
                    _ => null
                })
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToList();
        }

        public async Task<List<Category>> GetCategoriesForPatternsAsync(List<Guid> musicPatterns)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var categories = new List<Category>();

            foreach (var patternId in musicPatterns)
            {
                var patternCategories = await context.PatternCategories
                    .AsNoTracking()
                    .Where(pc => pc.MusicPatternId == patternId)
                    .Include(pc => pc.Category)
                    .ToListAsync();

                categories.AddRange(patternCategories.Select(pc => pc.Category).Where(c => c != null)!);
            }

            return categories;
        }

        public async Task<List<AudioMetadata>> GetScheduledSongsAsync(List<Category> categories, Dictionary<string, int> categoryRotationIndex)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var scheduledSongs = new List<AudioMetadata>();

            foreach (var category in categories)
            {
                var audioFiles = await context.AudioFiles
                    .AsNoTracking()
                    .Where(af => af.SelectedCategory == category.CategoryName)
                    .ToListAsync();

                if (audioFiles.Any())
                {
                    if (!categoryRotationIndex.TryGetValue(category.CategoryName ?? string.Empty, out int lastIndex))
                    {
                        lastIndex = 0;
                    }

                    var nextIndex = (lastIndex + 1) % audioFiles.Count;
                    var rotatedSong = audioFiles[nextIndex];

                    categoryRotationIndex[category.CategoryName ?? string.Empty] = nextIndex;

                    scheduledSongs.Add(rotatedSong);
                }
            }

            return scheduledSongs;
        }

        public async Task AddNewDayLogToDbLogAsync(List<ProgramLogItem> newDaysLog)
        {
            using var context = _dbContextFactory.CreateDbContext();
            if (newDaysLog == null || !newDaysLog.Any())
            {
                Log.Error("+++ AppDatabaseService - AddNewDayLogToDbLogAsync() -- NewDaysLog is null or empty.");
                return;
            }

            var newLogDate = newDaysLog.First().Date;

            int maxLogOrderIDForPreviousDate = await context.Log
                .AsNoTracking()
                .Where(log => log.Date < newLogDate)
                .MaxAsync(log => (int?)log.LogOrderID) ?? 0;

            int currentLogOrderID = maxLogOrderIDForPreviousDate;
            foreach (var logItem in newDaysLog)
            {
                logItem.LogOrderID = ++currentLogOrderID;
            }

            await context.Log.AddRangeAsync(newDaysLog);
            await context.SaveChangesAsync();

            var logsToRenumber = await context.Log
                .Where(log => log.Date > newLogDate)
                .OrderBy(log => log.Date)
                .ThenBy(log => log.LogOrderID)
                .ToListAsync();

            currentLogOrderID = newDaysLog.Max(log => log.LogOrderID);
            foreach (var logItem in logsToRenumber)
            {
                logItem.LogOrderID = ++currentLogOrderID;
            }

            context.Log.UpdateRange(logsToRenumber);
            await context.SaveChangesAsync();
        }

        public async Task<List<MusicGridItem>> GetMusicGridItemsAsync(Guid stationId)
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.MusicGridItems.Where(m => m.StationId == stationId).ToListAsync();
        }

        public async Task AddMusicGridItemAsync(MusicGridItem item)
        {
            using var context = _dbContextFactory.CreateDbContext();
            await context.MusicGridItems.AddAsync(item);
            await context.SaveChangesAsync();
        }

        public async Task<List<MusicPattern>> GetMusicPatternsAsync(Guid stationId)
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.MusicPatterns.Where(m => m.StationId == stationId).ToListAsync();
        }

        public async Task<List<AudioMetadata>> GetAudioFilesAsync()
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.AudioFiles.ToListAsync();
        }

        public async Task<AudioMetadata?> GetAudioFileByIdAsync(int id)
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.AudioFiles.FindAsync(id);
        }

        public async Task AddAudioFileAsync(AudioMetadata audioMetadata)
        {
            using var context = _dbContextFactory.CreateDbContext();
            await context.AudioFiles.AddAsync(audioMetadata);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAudioFileAsync(AudioMetadata audioMetadata)
        {
            using var context = _dbContextFactory.CreateDbContext();
            context.AudioFiles.Remove(audioMetadata);
            await context.SaveChangesAsync();
        }

        public async Task<List<Folder>> GetFoldersForStationAsync(Guid stationId)
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.Folders.Where(f => f.StationId == stationId).ToListAsync();
        }

        public async Task<Folder?> GetFolderByIdAsync(int id)
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.Folders.FindAsync(id);
        }

        public async Task AddFolderAsync(Folder folder)
        {
            using var context = _dbContextFactory.CreateDbContext();
            await context.Folders.AddAsync(folder);
            await context.SaveChangesAsync();
        }

        public async Task DeleteFolderAsync(Folder folder)
        {
            using var context = _dbContextFactory.CreateDbContext();
            context.Folders.Remove(folder);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAudioMetadataAsync(AudioMetadata audioMetadata)
        {
            using var context = _dbContextFactory.CreateDbContext();
            context.AudioFiles.Update(audioMetadata);
            await context.SaveChangesAsync();
        }

        public async Task UpdateMusicGridItemAsync(MusicGridItem musicGridItem)
        {
            using var context = _dbContextFactory.CreateDbContext();
            context.MusicGridItems.Update(musicGridItem);
            await context.SaveChangesAsync();
        }

        public async Task<AppSettings?> GetAppSettingsAsync()
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.AppSettings.FirstOrDefaultAsync();
        }

        public async Task SaveAppSettingsAsync(AppSettings appSettings)
        {
            using var context = _dbContextFactory.CreateDbContext();
            context.AppSettings.Update(appSettings);
            await context.SaveChangesAsync();
        }

        public async Task<List<ProgramLogItem>> GetProgramLogItemsAsync(Guid stationId)
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.Log
                .Where(log => log.StationId == stationId)
                .OrderBy(log => log.LogOrderID)
                .ToListAsync();
        }

        public async Task<bool> HasLogEntriesAsync(Guid stationId)
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.Log.AnyAsync(li => li.StationId == stationId);
        }

        public async Task AddProgramLogItemAsync(ProgramLogItem logItem)
        {
            using var context = _dbContextFactory.CreateDbContext();
            await context.Log.AddAsync(logItem);
            await context.SaveChangesAsync();
        }

        public async Task RemoveProgramLogItemAsync(ProgramLogItem logItem)
        {
            using var context = _dbContextFactory.CreateDbContext();
            context.Log.Remove(logItem);
            await context.SaveChangesAsync();
        }

        public async Task<List<string>> GetMusicPatternNamesAsync()
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.MusicPatterns.Select(mp => mp.Name!).ToListAsync();
        }

        public async Task<List<string>> GetCategoryNamesAsync()
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.Categories.Select(c => c.CategoryName!).ToListAsync();
        }

        public async Task<List<Category>> GetSelectedPatternCategoriesAsync(Guid musicPatternId)
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.PatternCategories
                .Where(pc => pc.MusicPatternId == musicPatternId)
                .OrderBy(pc => pc.MusicPatternSortOrder)
                .Select(pc => pc.Category)
                .ToListAsync();
        }

        public async Task AddMusicPatternAsync(MusicPattern musicPattern)
        {
            using var context = _dbContextFactory.CreateDbContext();
            await context.MusicPatterns.AddAsync(musicPattern);
            await context.SaveChangesAsync();
        }

        public async Task<MusicPattern?> GetMusicPatternByNameAsync(string name)
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.MusicPatterns.FirstOrDefaultAsync(mp => mp.Name == name);
        }

        public async Task DeleteMusicPatternAsync(MusicPattern musicPattern)
        {
            using var context = _dbContextFactory.CreateDbContext();
            context.MusicPatterns.Remove(musicPattern);
            await context.SaveChangesAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(Guid categoryId)
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.Categories.FindAsync(categoryId);
        }

        public async Task AddCategoryToPatternAsync(MusicPattern musicPattern, Category category)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var maxSortOrder = musicPattern.PatternCategories.Any()
                ? musicPattern.PatternCategories.Max(pc => pc.MusicPatternSortOrder)
                : 0;

            var patternCategory = new PatternCategory
            {
                MusicPatternId = musicPattern.PatternId,
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName!,
                MusicPatternSortOrder = maxSortOrder + 1,
                StationId = musicPattern.StationId
            };

            musicPattern.PatternCategories.Add(patternCategory);
            await context.SaveChangesAsync();
        }

        public async Task RemoveCategoryFromPatternAsync(MusicPattern musicPattern, Category category)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var patternCategory = await context.PatternCategories.FirstOrDefaultAsync(pc => pc.CategoryId == category.CategoryId && pc.MusicPatternId == musicPattern.PatternId);
            if (patternCategory != null)
            {
                context.PatternCategories.Remove(patternCategory);
                await context.SaveChangesAsync();
            }
        }

        public async Task MoveCategoryUpAsync(MusicPattern musicPattern, Category category)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var currentPatternCategory = await context.PatternCategories
                .FirstOrDefaultAsync(pc => pc.CategoryId == category.CategoryId && pc.MusicPatternId == musicPattern.PatternId);

            if (currentPatternCategory != null && currentPatternCategory.MusicPatternSortOrder > 1)
            {
                var previousPatternCategory = await context.PatternCategories
                    .FirstOrDefaultAsync(pc => pc.MusicPatternId == currentPatternCategory.MusicPatternId && pc.MusicPatternSortOrder == currentPatternCategory.MusicPatternSortOrder - 1);

                if (previousPatternCategory != null)
                {
                    int tempOrder = currentPatternCategory.MusicPatternSortOrder;
                    currentPatternCategory.MusicPatternSortOrder = previousPatternCategory.MusicPatternSortOrder;
                    previousPatternCategory.MusicPatternSortOrder = tempOrder;

                    await context.SaveChangesAsync();
                }
            }
        }

        public async Task MoveCategoryDownAsync(MusicPattern musicPattern, Category category)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var currentPatternCategory = await context.PatternCategories
                .FirstOrDefaultAsync(pc => pc.CategoryId == category.CategoryId && pc.MusicPatternId == musicPattern.PatternId);

            if (currentPatternCategory != null)
            {
                var nextPatternCategory = await context.PatternCategories
                    .FirstOrDefaultAsync(pc => pc.MusicPatternId == currentPatternCategory.MusicPatternId && pc.MusicPatternSortOrder == currentPatternCategory.MusicPatternSortOrder + 1);

                if (nextPatternCategory != null)
                {
                    int tempOrder = currentPatternCategory.MusicPatternSortOrder;
                    currentPatternCategory.MusicPatternSortOrder = nextPatternCategory.MusicPatternSortOrder;
                    nextPatternCategory.MusicPatternSortOrder = tempOrder;

                    await context.SaveChangesAsync();
                }
            }
        }

        public async Task<List<Category>> GetSelectedPatternCategoriesAsync(Guid stationId, string patternName)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var musicPattern = await context.MusicPatterns
                .Include(mp => mp.PatternCategories)
                .ThenInclude(pc => pc.Category)
                .FirstOrDefaultAsync(mp => mp.Name == patternName && mp.StationId == stationId);

            return musicPattern?.PatternCategories
                .OrderBy(pc => pc.MusicPatternSortOrder)
                .Select(pc => pc.Category)
                .ToList() ?? new List<Category>();
        }
    }
}

