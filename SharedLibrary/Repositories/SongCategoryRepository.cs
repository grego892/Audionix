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

        public async Task<List<SongCategory>> GetSongCategoriesAsync(Guid stationId)
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.SongCategories.Where(c => c.StationId == stationId).ToListAsync();
        }

        public async Task AddSongCategoryAsync(SongCategory songCategory)
        {
            using var context = _dbContextFactory.CreateDbContext();
            await context.SongCategories.AddAsync(songCategory);
            await context.SaveChangesAsync();
        }

        public async Task DeleteSongCategoryAsync(Guid songCategoryId)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var songCategory = await context.SongCategories.FindAsync(songCategoryId);
            if (songCategory != null)
            {
                context.SongCategories.Remove(songCategory);
                await context.SaveChangesAsync();
            }
        }

        public async Task<SongCategory?> GetSongCategoryByIdAsync(Guid songCategoryId)
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.SongCategories.FindAsync(songCategoryId);
        }

        public async Task<List<string>> GetSongCategoryNamesAsync()
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.SongCategories.Select(c => c.SongCategoryName!).ToListAsync();
        }

        public async Task<List<SongCategory>> GetSongCategoriesForPatternsAsync(List<Guid> musicPatterns)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var songCategories = new List<SongCategory>();

            foreach (var patternId in musicPatterns)
            {
                var patternCategories = await context.PatternCategories
                    .AsNoTracking()
                    .Where(pc => pc.MusicPatternId == patternId)
                    .Include(pc => pc.SongCategory)
                    .ToListAsync();

                foreach (var patternCategory in patternCategories)
                {
                    if (patternCategory.SongCategory != null)
                    {
                        songCategories.Add(patternCategory.SongCategory);
                    }
                }
            }

            return songCategories;
        }
    }
}