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

        public async Task<List<MusicPattern>> GetMusicPatternsAsync(int stationId)
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

        public async Task AddSongCategoryToPatternAsync(int musicPatternId, int? categoryId)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var musicPattern = await context.MusicPatterns.Include(mp => mp.PatternCategories).FirstOrDefaultAsync(mp => mp.PatternId == musicPatternId);
            var category = await context.Categories.FirstOrDefaultAsync(c => c.CategoryId == categoryId);

            if (musicPattern != null && category != null)
            {
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
        }

        public async Task RemoveSongCategoryFromPatternAsync(MusicPattern musicPattern, PatternCategory patternCategory)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var patternCategoryToRemove = await context.PatternCategories.FirstOrDefaultAsync(pc => pc.MusicPatternId == musicPattern.PatternId && pc.CategoryId == patternCategory.CategoryId);
            if (patternCategoryToRemove != null)
            {
                context.PatternCategories.Remove(patternCategoryToRemove);
                await context.SaveChangesAsync();
            }
        }

        public async Task MoveSongCategoryUpAsync(MusicPattern musicPattern, PatternCategory patternCategory)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var currentPatternCategory = await context.PatternCategories
                .FirstOrDefaultAsync(pc => pc.MusicPatternId == musicPattern.PatternId && pc.CategoryId == patternCategory.CategoryId);

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

        public async Task MoveSongCategoryDownAsync(MusicPattern musicPattern, PatternCategory patternCategory)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var currentPatternCategory = await context.PatternCategories
                .FirstOrDefaultAsync(pc => pc.MusicPatternId == musicPattern.PatternId && pc.CategoryId == patternCategory.CategoryId);

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

        public async Task<List<PatternCategory>> GetSelectedPatternCategoriesAsync(int musicPatternId)
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.PatternCategories
                .Where(pc => pc.MusicPatternId == musicPatternId)
                .OrderBy(pc => pc.MusicPatternSortOrder)
                .ToListAsync();
        }

        public async Task<List<PatternCategory>> GetSelectedPatternCategoriesAsync(int stationId, string patternName)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var musicPattern = await context.MusicPatterns
                .Include(mp => mp.PatternCategories)
                .FirstOrDefaultAsync(mp => mp.Name == patternName && mp.StationId == stationId);

            return musicPattern?.PatternCategories
                .OrderBy(pc => pc.MusicPatternSortOrder)
                .ToList() ?? new List<PatternCategory>();
        }

        public async Task<List<string>> GetMusicPatternNamesAsync()
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.MusicPatterns.Select(mp => mp.Name!).ToListAsync();
        }

        public async Task<List<int>> GetMusicPatternsForDayAsync(int stationId, DayOfWeek day)
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
                _ => null
            }).Where(id => id.HasValue).Select(id => id!.Value).ToList();

            return patternIds;
        }

        public async Task<List<MusicGridItem>> GetMusicGridItemsAsync(int stationId)
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

        public async Task<int> GetNextPatternIdAsync()
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.MusicPatterns.MaxAsync(mp => (int?)mp.PatternId) + 1 ?? 1;
        }
    }
}

