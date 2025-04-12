using SharedLibrary.Data;
using SharedLibrary.Models;
using SharedLibrary.Models.MusicSchedule;
using Microsoft.EntityFrameworkCore;
using Serilog;

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

        public async Task<List<AudioMetadata>> GetScheduledSongsAsync(
            List<Category> songCategories,
            Dictionary<string, int> songCategoryRotationIndex)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var scheduledSongs = new List<AudioMetadata>();

            // Log the start of the scheduling process
            Log.Information("Starting GetScheduledSongsAsync with {SongCategoryCount} categories.", songCategories.Count);

            // Retrieve SongScheduleSettings
            var settings = await context.SongScheduleSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                Log.Error("SongScheduleSettings not found. Cannot proceed with scheduling.");
                throw new InvalidOperationException("SongScheduleSettings not found.");
            }

            // Separation settings
            int artistSeparation = settings.ArtistSeperation;
            int titleSeparation = settings.TitleSeperation;
            int maxSoundcodeSeparation = settings.MaxSoundcodeSeperation;
            int maxEnergySeparation = settings.MaxEnergySeperation;

            Log.Information("Loaded SongScheduleSettings: ArtistSeparation={ArtistSeparation}, TitleSeparation={TitleSeparation}, MaxSoundcodeSeparation={MaxSoundcodeSeparation}, MaxEnergySeparation={MaxEnergySeparation}.",
                artistSeparation, titleSeparation, maxSoundcodeSeparation, maxEnergySeparation);

            // Track recently scheduled items
            var recentArtists = new Queue<string>();
            var recentTitles = new Queue<string>();
            var recentSoundCodes = new Queue<int?>();
            var recentEnergyLevels = new Queue<int?>();

            foreach (var songCategory in songCategories)
            {
                Log.Information("Processing song category: {CategoryName} (ID: {CategoryId}).", songCategory.CategoryName, songCategory.CategoryId);

                // Retrieve audio files for the current category
                var audioFiles = await context.AudioFiles
                    .AsNoTracking()
                    .Where(af => af.CategoryId == songCategory.CategoryId)
                    .ToListAsync();

                if (!audioFiles.Any())
                {
                    Log.Warning("No audio files found for category: {CategoryName} (ID: {CategoryId}).", songCategory.CategoryName, songCategory.CategoryId);
                    continue;
                }

                if (!songCategoryRotationIndex.TryGetValue(songCategory.CategoryName ?? string.Empty, out int lastIndex))
                {
                    lastIndex = 0;
                }

                // Find the next valid song based on separation rules
                AudioMetadata? nextSong = null;
                for (int i = 0; i < audioFiles.Count; i++)
                {
                    var candidateIndex = (lastIndex + i) % audioFiles.Count;
                    var candidateSong = audioFiles[candidateIndex];

                    // Log the candidate song being evaluated
                    Log.Debug("Evaluating song: {SongTitle} by {Artist} (ID: {SongId}).", candidateSong.Title, candidateSong.Artist, candidateSong.Id);

                    // Check separation rules
                    if (recentArtists.Contains(candidateSong.Artist))
                    {
                        Log.Debug("Skipping song {SongTitle} due to artist separation rule.", candidateSong.Title);
                        continue;
                    }

                    if (recentTitles.Contains(candidateSong.Title))
                    {
                        Log.Debug("Skipping song {SongTitle} due to title separation rule.", candidateSong.Title);
                        continue;
                    }

                    if (recentSoundCodes.Contains(candidateSong.SoundCodeId))
                    {
                        Log.Debug("Skipping song {SongTitle} due to sound code separation rule.", candidateSong.Title);
                        continue;
                    }

                    if (recentEnergyLevels.Contains(candidateSong.EnergyLevelId))
                    {
                        Log.Debug("Skipping song {SongTitle} due to energy level separation rule.", candidateSong.Title);
                        continue;
                    }

                    // If all rules are satisfied, select this song
                    nextSong = candidateSong;
                    lastIndex = candidateIndex;
                    break;
                }

                if (nextSong != null)
                {
                    // Update rotation index
                    songCategoryRotationIndex[songCategory.CategoryName ?? string.Empty] = lastIndex;

                    // Add the song to the scheduled list
                    scheduledSongs.Add(nextSong);

                    // Log the selected song
                    Log.Information("Scheduled song: {SongTitle} by {Artist} (ID: {SongId}).", nextSong.Title, nextSong.Artist, nextSong.Id);

                    // Update recent queues
                    UpdateRecentQueue(recentArtists, nextSong.Artist, artistSeparation);
                    UpdateRecentQueue(recentTitles, nextSong.Title, titleSeparation);
                    UpdateRecentQueue(recentSoundCodes, nextSong.SoundCodeId, maxSoundcodeSeparation);
                    UpdateRecentQueue(recentEnergyLevels, nextSong.EnergyLevelId, maxEnergySeparation);
                }
                else
                {
                    Log.Warning("No valid song found for category: {CategoryName} (ID: {CategoryId}) after applying separation rules.", songCategory.CategoryName, songCategory.CategoryId);
                }
            }

            // Log the completion of the scheduling process
            Log.Information("Completed GetScheduledSongsAsync. Scheduled {ScheduledSongCount} songs.", scheduledSongs.Count);

            return scheduledSongs;
        }

        // Helper method to update recent queues
        private void UpdateRecentQueue<T>(Queue<T> queue, T item, int maxSize)
        {
            if (item == null) return;

            queue.Enqueue(item);
            if (queue.Count > maxSize)
            {
                queue.Dequeue();
            }

            // Log the update to the recent queue
            Log.Debug("Updated recent queue for type {TypeName}. Current size: {QueueSize}.", typeof(T).Name, queue.Count);
        }

        public async Task<AudioMetadata?> GetAudioFileByFilenameAsync(string filename)
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.AudioFiles.AsNoTracking().FirstOrDefaultAsync(am => am.Filename == filename);
        }
    }
}

