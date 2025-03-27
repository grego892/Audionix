using SharedLibrary.Data;
using SharedLibrary.Models.MusicSchedule;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Models;

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

        public async Task<SongCategory?> GetSongCategoryByIdAsync(int songCategoryId)
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.SongCategories.FindAsync(songCategoryId);
        }

        public async Task<List<string>> GetSongCategoryNamesAsync()
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.SongCategories.Select(c => c.SongCategoryName!).ToListAsync();
        }

        public async Task<List<AudioMetadata>> GetScheduledSongsAsync(List<SongCategory> songCategories, Dictionary<string, int> songCategoryRotationIndex)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var scheduledSongs = new List<AudioMetadata>();

            foreach (var songCategory in songCategories)
            {
                var audioFiles = await context.AudioFiles
                    .AsNoTracking()
                    .Where(af => af.CategoryId == songCategory.SongCategoryId)
                    .ToListAsync();

                if (audioFiles.Any())
                {
                    if (!songCategoryRotationIndex.TryGetValue(songCategory.SongCategoryName ?? string.Empty, out int lastIndex))
                    {
                        lastIndex = 0;
                    }

                    var nextIndex = (lastIndex + 1) % audioFiles.Count; // Fixed the issue with the modulus operator
                    var rotatedSong = audioFiles[nextIndex];

                    songCategoryRotationIndex[songCategory.SongCategoryName ?? string.Empty] = nextIndex;

                    scheduledSongs.Add(rotatedSong);
                }
            }

            return scheduledSongs;
        }
    }
}