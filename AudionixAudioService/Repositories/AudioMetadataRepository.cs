using AudionixAudioServer.Data;
using AudionixAudioServer.Models;
using AudionixAudioServer.Models.MusicSchedule;
using AudionixAudioServer.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AudionixAudioServer.Repositories
{
    public class AudioMetadataRepository : IAudioMetadataRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public AudioMetadataRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<AudioMetadata?> GetAudioFileByFilenameAsync(string filename)
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.AudioFiles.AsNoTracking().FirstOrDefaultAsync(am => am.Filename == filename);
        }

        // Ensure other methods are implemented as needed
        // public async Task<List<AudioMetadata>> GetScheduledSongsAsync(List<Category> categories, Dictionary<string, int> categoryRotationIndex)
        // {
        //     // Implementation
        // }

        // public async Task<List<AudioMetadata>> GetAudioFilesAsync()
        // {
        //     using var context = _dbContextFactory.CreateDbContext();
        //     return await context.AudioFiles.ToListAsync();
        // }

        // public async Task<AudioMetadata?> GetAudioFileByIdAsync(int id)
        // {
        //     using var context = _dbContextFactory.CreateDbContext();
        //     return await context.AudioFiles.FindAsync(id);
        // }

        // public async Task AddAudioFileAsync(AudioMetadata audioMetadata)
        // {
        //     using var context = _dbContextFactory.CreateDbContext();
        //     await context.AudioFiles.AddAsync(audioMetadata);
        //     await context.SaveChangesAsync();
        // }

        // public async Task DeleteAudioFileAsync(AudioMetadata audioMetadata)
        // {
        //     using var context = _dbContextFactory.CreateDbContext();
        //     context.AudioFiles.Remove(audioMetadata);
        //     await context.SaveChangesAsync();
        // }

        // public async Task UpdateAudioMetadataAsync(AudioMetadata audioMetadata)
        // {
        //     using var context = _dbContextFactory.CreateDbContext();
        //     context.AudioFiles.Update(audioMetadata);
        //     await context.SaveChangesAsync();
        // }
    }
}

