using SharedLibrary.Data;
using SharedLibrary.Models;
using SharedLibrary.Models.MusicSchedule;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace SharedLibrary.Repositories
{
    public class StationRepository : IStationRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public StationRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<List<Station>> GetStationsAsync()
        {
            try
            {
                using var context = _dbContextFactory.CreateDbContext();
                return await context.Stations.ToListAsync();
            }
            catch (Npgsql.NpgsqlException ex)
            {
                Log.Error(ex, "++++++ StationRepository.cs - PostgreSQL is not running or not set up correctly.");
                return new List<Station>();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "++++++ StationRepository.cs - An error occurred while getting stations.");
                return new List<Station>();
            }
        }

        public async Task<Station?> GetStationByIdAsync(Guid stationId)
        {
            try
            {
                using var context = _dbContextFactory.CreateDbContext();
                return await context.Stations.AsNoTracking().FirstOrDefaultAsync(s => s.StationId == stationId);
            }
            catch (Npgsql.NpgsqlException ex)
            {
                Log.Error(ex, "++++++ StationRepository.cs - PostgreSQL is not running or not set up correctly.");
                return null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "++++++ StationRepository.cs - An error occurred while getting station by ID.");
                return null;
            }
        }

        public async Task AddStationAsync(Station station)
        {
            try
            {
                using var context = _dbContextFactory.CreateDbContext();
                await context.Stations.AddAsync(station);
                await context.SaveChangesAsync();
            }
            catch (Npgsql.NpgsqlException ex)
            {
                Log.Error(ex, "++++++ StationRepository.cs - PostgreSQL is not running or not set up correctly.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "++++++ StationRepository.cs - An error occurred while adding a station.");
            }
        }

        public async Task UpdateStationAsync(Station station)
        {
            try
            {
                using var context = _dbContextFactory.CreateDbContext();
                context.Stations.Update(station);
                await context.SaveChangesAsync();
            }
            catch (Npgsql.NpgsqlException ex)
            {
                Log.Error(ex, "++++++ StationRepository.cs - PostgreSQL is not running or not set up correctly.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "++++++ StationRepository.cs - An error occurred while updating a station.");
            }
        }

        public async Task DeleteStationAsync(Guid stationId)
        {
            try
            {
                using var context = _dbContextFactory.CreateDbContext();
                var station = await context.Stations.FindAsync(stationId);
                if (station != null)
                {
                    context.Stations.Remove(station);
                    await context.SaveChangesAsync();
                }
            }
            catch (Npgsql.NpgsqlException ex)
            {
                Log.Error(ex, "++++++ StationRepository.cs - PostgreSQL is not running or not set up correctly.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "++++++ StationRepository.cs - An error occurred while deleting a station.");
            }
        }

        public async Task<List<SongCategory>> GetSongCategoriesAsync(Guid stationId)
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.SongCategories.Where(c => c.StationId == stationId).ToListAsync();
        }

        public async Task AddSongCategoryAsync(SongCategory songCategory)
        {
            using var context = _dbContextFactory.CreateDbContext();
            context.SongCategories.Add(songCategory);
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

        public async Task<List<Guid>> GetMusicPatternsForDayAsync(Guid stationId, DayOfWeek day)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var musicGridItems = await context.MusicGridItems
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

        public async Task<List<SongCategory>> GetSongCategoriesForPatternsAsync(List<int> musicPatterns)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var songCategories = new List<SongCategory>();

            foreach (var patternId in musicPatterns)
            {
                var patternCategories = await context.PatternCategories
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

        public async Task<List<AudioMetadata>> GetScheduledSongsAsync(List<SongCategory> songCategories, Dictionary<string, int> songCategoryRotationIndex)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var scheduledSongs = new List<AudioMetadata>();

            foreach (var songCategory in songCategories)
            {
                var audioFiles = await context.AudioFiles
                    .Where(af => af.CategoryId == songCategory.SongCategoryId) // Fixed the property name
                    .ToListAsync();

                if (audioFiles.Any())
                {
                    // Get the last used index for this category
                    if (!songCategoryRotationIndex.TryGetValue(songCategory.SongCategoryName ?? string.Empty, out int lastIndex))
                    {
                        lastIndex = 0;
                    }

                    // Get the next song in the rotation
                    var nextIndex = (lastIndex + 1) % audioFiles.Count; // Fixed the issue with the modulus operator

                    // Update the last used index for this song category
                    songCategoryRotationIndex[songCategory.SongCategoryName ?? string.Empty] = nextIndex;

                    // Add the rotated song to the scheduled songs
                    scheduledSongs.Add(audioFiles[nextIndex]);
                }
            }

            return scheduledSongs;
        }

        public async Task AddNewDayLogToDbLogAsync(List<ProgramLogItem> newDaysLog)
        {
            if (newDaysLog != null && newDaysLog.Count > 0)
            {
                using var context = _dbContextFactory.CreateDbContext();
                var newLogDate = newDaysLog.First().Date;

                // Assign LogOrderID to the new log items starting from 1
                int currentLogOrderID = 0;
                foreach (var logItem in newDaysLog)
                {
                    logItem.LogOrderID = ++currentLogOrderID;
                }

                // Insert the new log items into the database
                await context.Log.AddRangeAsync(newDaysLog);
                await context.SaveChangesAsync();
            }
            else
            {
                Log.Error("+++ StationRepository - AddNewDayLogToDbLogAsync() -- NewDaysLog is null or empty.");
            }
        }

        public async Task<List<MusicGridItem>> GetMusicGridItemsAsync(Guid stationId)
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.MusicGridItems.Where(mgi => mgi.StationId == stationId).ToListAsync();
        }

        public async Task AddMusicGridItemAsync(MusicGridItem item)
        {
            using var context = _dbContextFactory.CreateDbContext();
            context.MusicGridItems.Add(item);
            await context.SaveChangesAsync();
        }

        public async Task<List<MusicPattern>> GetMusicPatternsAsync(Guid stationId)
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.MusicPatterns.Where(mp => mp.StationId == stationId).ToListAsync();
        }

        public async Task<List<AudioMetadata>> GetAudioFilesAsync()
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.AudioFiles.AsNoTracking().ToListAsync();
        }

        public async Task<AudioMetadata?> GetAudioFileByIdAsync(int id)
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.AudioFiles.AsNoTracking().FirstOrDefaultAsync(am => am.Id == id);
        }

        public async Task AddAudioFileAsync(AudioMetadata audioMetadata)
        {
            using var context = _dbContextFactory.CreateDbContext();
            context.AudioFiles.Add(audioMetadata);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAudioFileAsync(AudioMetadata audioMetadata)
        {
            using var context = _dbContextFactory.CreateDbContext();
            context.AudioFiles.Remove(audioMetadata);
            await context.SaveChangesAsync();
        }

        public async Task<List<Folder>> GetFoldersForStationAsync(Guid stationId)
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.Folders.Where(f => f.StationId == stationId).ToListAsync();
        }

        public async Task<Folder?> GetFolderByIdAsync(int id)
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.Folders.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task AddFolderAsync(Folder folder)
        {
            using var context = _dbContextFactory.CreateDbContext();
            context.Folders.Add(folder);
            await context.SaveChangesAsync();
        }

        public async Task DeleteFolderAsync(Folder folder)
        {
            using var context = _dbContextFactory.CreateDbContext();
            context.Folders.Remove(folder);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAudioMetadataAsync(AudioMetadata audioMetadata)
        {
            using var context = _dbContextFactory.CreateDbContext();
            context.AudioFiles.Update(audioMetadata);
            await context.SaveChangesAsync();
        }

        public async Task UpdateMusicGridItemAsync(MusicGridItem musicGridItem)
        {
            using var context = _dbContextFactory.CreateDbContext();
            context.MusicGridItems.Update(musicGridItem);
            await context.SaveChangesAsync();
        }

        public async Task<AudioMetadata?> GetAudioFileByFilenameAsync(string filename)
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.AudioFiles.AsNoTracking().FirstOrDefaultAsync(am => am.Filename == filename);
        }

        public async Task<AppSettings?> GetAppSettingsDataPathAsync() // Fixed the return type to be nullable
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.AppSettings.FirstOrDefaultAsync();
        }

        public async Task UpdateProgramLogItemAsync(ProgramLogItem logItem)
        {
            using var context = _dbContextFactory.CreateDbContext();
            context.Log.Update(logItem);
            await context.SaveChangesAsync();
        }

        public async Task UpdateStationNextPlayAsync(Guid stationId, int logOrderID, DateOnly Date)
        {
            try
            {
                using var context = _dbContextFactory.CreateDbContext();
                var station = await context.Stations.FirstOrDefaultAsync(s => s.StationId == stationId);
                if (station != null)
                {
                    station.NextPlayId = logOrderID;
                    station.NextPlayDate = Date;
                    context.Stations.Update(station);
                    await context.SaveChangesAsync();
                }
            }
            catch (Npgsql.NpgsqlException ex)
            {
                Log.Error(ex, "++++++ StationRepository.cs - PostgreSQL is not running or not set up correctly.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "++++++ StationRepository.cs - An error occurred while updating station next play.");
            }
        }
    }
}
