using Audionix.Data;
using Audionix.Models;
using Audionix.Models.MusicSchedule;
using Microsoft.EntityFrameworkCore;
using Serilog;

public class AppDatabaseService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public AppDatabaseService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<AppSettings> GetAppSettingsAsync()
    {
        await using var context = _contextFactory.CreateDbContext();
        var appSettings = await context.AppSettings.FirstOrDefaultAsync();
        if (appSettings == null)
        {
            appSettings = new AppSettings();
            context.AppSettings.Add(appSettings);
            await context.SaveChangesAsync();
        }
        return appSettings;
    }

    public async Task AddStationToDataPath(Station station, AppSettings appSettings)
    {
        await using var context = _contextFactory.CreateDbContext();
        var existingAppSettings = await context.AppSettings.FirstOrDefaultAsync();
        if (existingAppSettings != null)
        {
            existingAppSettings.DataPath = appSettings.DataPath;
            context.AppSettings.Update(existingAppSettings);
            await context.SaveChangesAsync();
        }
    }

    public async Task SaveConfigurationAsync(AppSettings appSettings)
    {
        await using var context = _contextFactory.CreateDbContext();
        if (appSettings != null)
        {
            context.AppSettings.Update(appSettings);
            await context.SaveChangesAsync();
        }
    }

    public async Task<List<Station>> GetStationsAsync()
    {
        await using var context = _contextFactory.CreateDbContext();
        return await context.Stations.ToListAsync();
    }

    public async Task<Station?> GetStationByIdAsync(Guid stationId)
    {
        await using var context = _contextFactory.CreateDbContext();
        return await context.Stations.AsNoTracking().FirstOrDefaultAsync(s => s.StationId == stationId);
    }

    public async Task<List<Category>> GetCategoriesAsync(Guid stationId)
    {
        await using var context = _contextFactory.CreateDbContext();
        return await context.Categories.Where(c => c.StationId == stationId).ToListAsync();
    }

    public async Task AddCategoryAsync(Category category)
    {
        await using var context = _contextFactory.CreateDbContext();
        context.Categories.Add(category);
        await context.SaveChangesAsync();
    }

    public async Task DeleteCategoryAsync(Guid categoryId)
    {
        await using var context = _contextFactory.CreateDbContext();
        var category = await context.Categories.FindAsync(categoryId);
        if (category != null)
        {
            context.Categories.Remove(category);
            await context.SaveChangesAsync();
        }
    }

    public async Task<List<Guid>> GetMusicPatternsForDayAsync(Guid stationId, DayOfWeek day)
    {
        await using var context = _contextFactory.CreateDbContext();
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

    public async Task<List<Category>> GetCategoriesForPatternsAsync(List<Guid> musicPatterns)
    {
        await using var context = _contextFactory.CreateDbContext();
        var categories = new List<Category>();

        foreach (var patternId in musicPatterns)
        {
            var patternCategories = await context.PatternCategories
                .Where(pc => pc.MusicPatternId == patternId)
                .Include(pc => pc.Category)
                .ToListAsync();

            foreach (var patternCategory in patternCategories)
            {
                if (patternCategory.Category != null)
                {
                    categories.Add(patternCategory.Category);
                }
            }
        }

        return categories;
    }

    public async Task<List<AudioMetadata>> GetScheduledSongsAsync(List<Category> categories, Dictionary<string, int> categoryRotationIndex)
    {
        await using var context = _contextFactory.CreateDbContext();
        var scheduledSongs = new List<AudioMetadata>();

        foreach (var category in categories)
        {
            var audioFiles = await context.AudioFiles
                .Where(af => af.SelectedCategory == category.CategoryName)
                .ToListAsync();

            if (audioFiles.Any())
            {
                // Get the last used index for this category
                if (!categoryRotationIndex.TryGetValue(category.CategoryName ?? string.Empty, out int lastIndex))
                {
                    lastIndex = 0;
                }

                // Get the next song in the rotation
                var nextIndex = (lastIndex + 1) % audioFiles.Count;
                var rotatedSong = audioFiles[nextIndex];

                // Update the last used index for this category
                categoryRotationIndex[category.CategoryName ?? string.Empty] = nextIndex;

                // Add the rotated song to the scheduled songs
                scheduledSongs.Add(rotatedSong);
            }
        }

        return scheduledSongs;
    }

    public async Task AddNewDayLogToDbLogAsync(List<ProgramLogItem> newDaysLog)
    {
        if (newDaysLog != null && newDaysLog.Count > 0)
        {
            await using var context = _contextFactory.CreateDbContext();

            if (newDaysLog == null || !newDaysLog.Any())
            {
                Log.Error("+++ AppDatabaseService - AddNewDayLogToDbLogAsync() -- NewDaysLog is null or empty.");
            }

            var newLogDate = newDaysLog.First().Date;

            // Retrieve the maximum LogOrderID for the previous date in the log
            int maxLogOrderIDForPreviousDate = await context.Log
                .Where(log => log.Date < newLogDate)
                .MaxAsync(log => (int?)log.LogOrderID) ?? 0;

            // Assign LogOrderID to the new log items starting from the retrieved maximum LogOrderID
            int currentLogOrderID = maxLogOrderIDForPreviousDate;
            foreach (var logItem in newDaysLog)
            {
                logItem.LogOrderID = ++currentLogOrderID;
            }

            // Insert the new log items into the database
            await context.Log.AddRangeAsync(newDaysLog);
            await context.SaveChangesAsync();

            // Renumber all log items for dates after the new log items' date
            var logsToRenumber = await context.Log
                .Where(log => log.Date > newLogDate)
                .OrderBy(log => log.Date)
                .ThenBy(log => log.LogOrderID)
                .ToListAsync();

            currentLogOrderID = newDaysLog.Max(log => log.LogOrderID);
            foreach (var logItem in logsToRenumber)
            {
                logItem.LogOrderID = ++currentLogOrderID;
            }

            context.Log.UpdateRange(logsToRenumber);
            await context.SaveChangesAsync();
        }
        else 
        {
            Log.Error("+++ AppDatabaseService - AddNewDayLogToDbLogAsync() -- NewDaysLog is null or empty.");
        }
    }

    public async Task<List<MusicGridItem>> GetMusicGridItemsAsync(Guid stationId)
    {
        await using var context = _contextFactory.CreateDbContext();
        return await context.MusicGridItems.Where(mgi => mgi.StationId == stationId).ToListAsync();
    }

    public async Task AddMusicGridItemAsync(MusicGridItem item)
    {
        await using var context = _contextFactory.CreateDbContext();
        context.MusicGridItems.Add(item);
        await context.SaveChangesAsync();
    }

    public async Task<List<MusicPattern>> GetMusicPatternsAsync(Guid stationId)
    {
        await using var context = _contextFactory.CreateDbContext();
        return await context.MusicPatterns.Where(mp => mp.StationId == stationId).ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await using var context = _contextFactory.CreateDbContext();
        await context.SaveChangesAsync();
    }

    public async Task<List<AudioMetadata>> GetAudioFilesAsync()
    {
        await using var context = _contextFactory.CreateDbContext();
        return await context.AudioFiles.AsNoTracking().ToListAsync();
    }

    public async Task<AudioMetadata?> GetAudioFileByIdAsync(int id)
    {
        await using var context = _contextFactory.CreateDbContext();
        return await context.AudioFiles.AsNoTracking().FirstOrDefaultAsync(am => am.Id == id);
    }

    public async Task AddAudioFileAsync(AudioMetadata audioMetadata)
    {
        await using var context = _contextFactory.CreateDbContext();
        context.AudioFiles.Add(audioMetadata);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAudioFileAsync(AudioMetadata audioMetadata)
    {
        await using var context = _contextFactory.CreateDbContext();
        context.AudioFiles.Remove(audioMetadata);
        await context.SaveChangesAsync();
    }

    public async Task<List<Folder>> GetFoldersForStationAsync(Guid stationId)
    {
        await using var context = _contextFactory.CreateDbContext();
        return await context.Folders.Where(f => f.StationId == stationId).ToListAsync();
    }

    public async Task<Folder?> GetFolderByIdAsync(int id)
    {
        await using var context = _contextFactory.CreateDbContext();
        return await context.Folders.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task AddFolderAsync(Folder folder)
    {
        await using var context = _contextFactory.CreateDbContext();
        context.Folders.Add(folder);
        await context.SaveChangesAsync();
    }

    public async Task DeleteFolderAsync(Folder folder)
    {
        await using var context = _contextFactory.CreateDbContext();
        context.Folders.Remove(folder);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAudioMetadataAsync(AudioMetadata audioMetadata)
    {
        await using var context = _contextFactory.CreateDbContext();
        context.AudioFiles.Update(audioMetadata);
        await context.SaveChangesAsync();
    }
    public async Task UpdateMusicGridItemAsync(MusicGridItem musicGridItem)
    {
        await using var context = _contextFactory.CreateDbContext();
        context.MusicGridItems.Update(musicGridItem);
        await context.SaveChangesAsync();
    }
}
