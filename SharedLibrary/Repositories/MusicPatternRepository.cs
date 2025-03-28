using SharedLibrary.Data;
using SharedLibrary.Models.MusicSchedule;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace SharedLibrary.Repositories
{
    public class MusicPatternRepository : IMusicPatternRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public MusicPatternRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<List<MusicPattern>> GetMusicPatternsAsync(Guid stationId)
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.MusicPatterns.Where(m => m.StationId == stationId).ToListAsync();
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

        public async Task AddSongCategoryToPatternAsync(int musicPatternId, int songCategoryId)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var musicPattern = await context.MusicPatterns.Include(mp => mp.PatternCategories).FirstOrDefaultAsync(mp => mp.PatternId == musicPatternId);
            var songCategory = await context.SongCategories.FirstOrDefaultAsync(c => c.SongCategoryId == songCategoryId);

            if (musicPattern != null && songCategory != null)
            {
                var maxSortOrder = musicPattern.PatternCategories.Any()
                    ? musicPattern.PatternCategories.Max(pc => pc.MusicPatternSortOrder)
                    : 0;

                var patternCategory = new PatternCategory
                {
                    MusicPatternId = musicPattern.PatternId,
                    SongCategoryId = songCategory.SongCategoryId,
                    SongCategoryName = songCategory.SongCategoryName!,
                    MusicPatternSortOrder = maxSortOrder + 1,
                    StationId = musicPattern.StationId
                };

                musicPattern.PatternCategories.Add(patternCategory);
                await context.SaveChangesAsync();
            }
        }

        public async Task RemoveSongCategoryFromPatternAsync(MusicPattern musicPattern, SongCategory songCategory)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var patternCategory = await context.PatternCategories.FirstOrDefaultAsync(pc => pc.SongCategoryId == songCategory.SongCategoryId && pc.MusicPatternId == musicPattern.PatternId);
            if (patternCategory != null)
            {
                context.PatternCategories.Remove(patternCategory);
                await context.SaveChangesAsync();
            }
        }

        public async Task MoveSongCategoryUpAsync(MusicPattern musicPattern, SongCategory songCategory)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var currentPatternCategory = await context.PatternCategories
                .FirstOrDefaultAsync(pc => pc.SongCategoryId == songCategory.SongCategoryId && pc.MusicPatternId == musicPattern.PatternId);

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

        public async Task MoveSongCategoryDownAsync(MusicPattern musicPattern, SongCategory songCategory)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var currentPatternCategory = await context.PatternCategories
                .FirstOrDefaultAsync(pc => pc.SongCategoryId == songCategory.SongCategoryId && pc.MusicPatternId == musicPattern.PatternId);

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

        public async Task<List<SongCategory>> GetSelectedPatternCategoriesAsync(int musicPatternId)
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.PatternCategories
                .Where(pc => pc.MusicPatternId == musicPatternId)
                .OrderBy(pc => pc.MusicPatternSortOrder)
                .Select(pc => pc.SongCategory)
                .ToListAsync();
        }

        public async Task<List<SongCategory>> GetSelectedPatternCategoriesAsync(Guid stationId, string patternName)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var musicPattern = await context.MusicPatterns
                .Include(mp => mp.PatternCategories)
                .ThenInclude(pc => pc.SongCategory)
                .FirstOrDefaultAsync(mp => mp.Name == patternName && mp.StationId == stationId);

            return musicPattern?.PatternCategories
                .OrderBy(pc => pc.MusicPatternSortOrder)
                .Select(pc => pc.SongCategory)
                .ToList() ?? new List<SongCategory>();
        }

        public async Task<List<string>> GetMusicPatternNamesAsync()
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.MusicPatterns.Select(mp => mp.Name!).ToListAsync();
        }

        public async Task<List<int>> GetMusicPatternsForDayAsync(Guid stationId, DayOfWeek day)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var musicGridItems = await context.MusicGridItems
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
                _ => (int?)null
            }).Where(id => id.HasValue).Select(id => id.Value).ToList();

            var musicPatterns = await context.MusicPatterns
                .Where(mp => patternIds.Contains(mp.PatternId))
                .Select(mp => mp.StationId)
                .ToListAsync();

            return musicPatterns;
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

        public async Task UpdateMusicGridItemAsync(MusicGridItem musicGridItem)
        {
            using var context = _dbContextFactory.CreateDbContext();
            context.MusicGridItems.Update(musicGridItem);
            await context.SaveChangesAsync();
        }
    }
}
