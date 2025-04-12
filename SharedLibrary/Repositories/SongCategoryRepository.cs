using SharedLibrary.Data;
using SharedLibrary.Models.MusicSchedule;
using Microsoft.EntityFrameworkCore;

namespace SharedLibrary.Repositories
{
    public class SongCategoryRepository : ISongCategoryRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public SongCategoryRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<List<Category>> GetSongCategoriesAsync(int stationId)
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.SongCategories.Where(c => c.StationId == stationId).ToListAsync();
        }

        public async Task AddSongCategoryAsync(Category songCategory)
        {
            using var context = _dbContextFactory.CreateDbContext();
            await context.SongCategories.AddAsync(songCategory);
            await context.SaveChangesAsync();
        }

        public async Task DeleteSongCategoryAsync(int songCategoryId)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var songCategory = await context.SongCategories.FindAsync(songCategoryId);
            if (songCategory != null)
            {
                context.SongCategories.Remove(songCategory);
                await context.SaveChangesAsync();
            }
        }

        public async Task<Category?> GetSongCategoryByIdAsync(int songCategoryId)
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.SongCategories.FindAsync(songCategoryId);
        }

        public async Task<List<string>> GetSongCategoryNamesAsync()
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.SongCategories.Select(c => c.CategoryName!).ToListAsync();
        }

        public async Task<List<Category>> GetSongCategoriesForPatternsAsync(List<int> musicPatterns)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var songCategories = new List<Category>();

            foreach (var patternId in musicPatterns)
            {
                var patternCategories = await context.PatternCategories
                    .AsNoTracking()
                    .Where(pc => pc.MusicPatternId == patternId)
                    .ToListAsync();

                foreach (var patternCategory in patternCategories)
                {
                    var songCategory = await context.Categories.FirstOrDefaultAsync(sc => sc.CategoryId == patternCategory.CategoryId);
                    if (songCategory != null)
                    {
                        songCategories.Add(songCategory);
                    }
                }

            }

            return songCategories;
        }
    }
}
