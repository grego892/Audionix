using SharedLibrary.Data;
using SharedLibrary.Models;
using SharedLibrary.Models.MusicSchedule;
using Microsoft.EntityFrameworkCore;

namespace SharedLibrary.Repositories
{
    public class AudioMetadataRepository : IAudioMetadataRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public AudioMetadataRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory)
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

        public async Task<List<AudioMetadata>> GetScheduledSongsAsync(List<SongCategory> songCategories, Dictionary<string, int> songCategoryRotationIndex)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var scheduledSongs = new List<AudioMetadata>();

            foreach (var songCategory in songCategories)
            {
                var audioFiles = await context.AudioFiles
                    .AsNoTracking()
                    .Where(af => af.Category != null && af.Category.CategoryId == songCategory.SongCategoryId)
                    .ToListAsync();

                if (audioFiles.Any())
                {
                    if (!songCategoryRotationIndex.TryGetValue(songCategory.SongCategoryName ?? string.Empty, out int lastIndex))
                    {
                        lastIndex = 0;
                    }

                    var nextIndex = (lastIndex + 1) % audioFiles.Count;
                    var rotatedSong = audioFiles[nextIndex];

                    songCategoryRotationIndex[songCategory.SongCategoryName ?? string.Empty] = nextIndex;

                    scheduledSongs.Add(rotatedSong);
                }
            }

            return scheduledSongs;
        }

        public async Task<AudioMetadata?> GetAudioFileByFilenameAsync(string filename)
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.AudioFiles.AsNoTracking().FirstOrDefaultAsync(am => am.Filename == filename);
        }
    }
}

