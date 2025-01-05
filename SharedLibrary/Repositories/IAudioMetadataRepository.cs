using SharedLibrary.Models;
using SharedLibrary.Models.MusicSchedule;

namespace SharedLibrary.Repositories
{
    public interface IAudioMetadataRepository
    {
        Task<List<AudioMetadata>> GetAudioFilesAsync();
        Task<AudioMetadata?> GetAudioFileByIdAsync(int id);
        Task AddAudioFileAsync(AudioMetadata audioMetadata);
        Task DeleteAudioFileAsync(AudioMetadata audioMetadata);
        Task UpdateAudioMetadataAsync(AudioMetadata audioMetadata);
        Task<List<AudioMetadata>> GetScheduledSongsAsync(List<SongCategory> songCategories, Dictionary<string, int> categoryRotationIndex);
    }
}
