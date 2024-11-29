using Audionix.Data;
using Audionix.Models;
using Audionix.Models.MusicSchedule;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace Audionix.Repositories
{
    public class AudioMetadataRepository : IAudioMetadataRepository
    {
        private readonly IDbContextFactory _dbContextFactory;

        public AudioMetadataRepository(IDbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
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

        public async Task UpdateAudioMetadataAsync(AudioMetadata audioMetadata)
        {
            using var context = _dbContextFactory.CreateDbContext();
            context.AudioFiles.Update(audioMetadata);
            await context.SaveChangesAsync();
        }

        public async Task<List<AudioMetadata>> GetScheduledSongsAsync(List<Category> categories, Dictionary<string, int> categoryRotationIndex)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var scheduledSongs = new List<AudioMetadata>();

            foreach (var category in categories)
            {
                var audioFiles = await context.AudioFiles
                    .AsNoTracking()
                    .Where(af => af.Category == category.CategoryName)
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
    }
}
