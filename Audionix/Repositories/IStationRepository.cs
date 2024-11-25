using Audionix.Models;
using Audionix.Models.MusicSchedule;
using Microsoft.EntityFrameworkCore.Internal;
using Serilog;
using System.Threading.Tasks;

namespace Audionix.Repositories
{
    public interface IStationRepository
    {
        Task<List<Station>> GetStationsAsync();
        Task<Station?> GetStationByIdAsync(Guid stationId);
        Task AddStationAsync(Station station);
        Task UpdateStationAsync(Station station);
        Task DeleteStationAsync(Guid stationId);
        Task<List<Category>> GetCategoriesAsync(Guid stationId);
        Task AddCategoryAsync(Category category);
        Task DeleteCategoryAsync(Guid categoryId);
        Task<List<Guid>> GetMusicPatternsForDayAsync(Guid stationId, DayOfWeek day);
        Task<List<Category>> GetCategoriesForPatternsAsync(List<Guid> musicPatterns);
        Task<List<AudioMetadata>> GetScheduledSongsAsync(List<Category> categories, Dictionary<string, int> categoryRotationIndex);
        Task AddNewDayLogToDbLogAsync(List<ProgramLogItem> newDaysLog);
        Task<List<MusicGridItem>> GetMusicGridItemsAsync(Guid stationId);
        Task AddMusicGridItemAsync(MusicGridItem item);
        Task<List<MusicPattern>> GetMusicPatternsAsync(Guid stationId);
        Task<List<AudioMetadata>> GetAudioFilesAsync();
        Task<AudioMetadata?> GetAudioFileByIdAsync(int id);
        Task AddAudioFileAsync(AudioMetadata audioMetadata);
        Task DeleteAudioFileAsync(AudioMetadata audioMetadata);
        Task<List<Folder>> GetFoldersForStationAsync(Guid stationId);
        Task<Folder?> GetFolderByIdAsync(int id);
        Task AddFolderAsync(Folder folder);
        Task DeleteFolderAsync(Folder folder);
        Task UpdateAudioMetadataAsync(AudioMetadata audioMetadata);
        Task UpdateMusicGridItemAsync(MusicGridItem musicGridItem);
        Task<AppSettings?> GetAppSettingsAsync();
        Task SaveAppSettingsAsync(AppSettings appSettings);
        Task<List<ProgramLogItem>> GetProgramLogItemsAsync(Guid stationId);
        Task<bool> HasLogEntriesAsync(Guid stationId);
        Task AddProgramLogItemAsync(ProgramLogItem logItem);
        Task RemoveProgramLogItemAsync(ProgramLogItem logItem);
        Task<List<string>> GetCategoryNamesAsync();
        Task<List<string>> GetMusicPatternNamesAsync();
        Task AddMusicPatternAsync(MusicPattern musicPattern);
        Task<MusicPattern?> GetMusicPatternByNameAsync(string name);
        Task DeleteMusicPatternAsync(MusicPattern musicPattern);
        Task<Category?> GetCategoryByIdAsync(Guid categoryId);
        Task AddCategoryToPatternAsync(MusicPattern musicPattern, Category category);
        Task RemoveCategoryFromPatternAsync(MusicPattern musicPattern, Category category);
        Task MoveCategoryUpAsync(MusicPattern musicPattern, Category category);
        Task MoveCategoryDownAsync(MusicPattern musicPattern, Category category);
        Task<List<Category>> GetSelectedPatternCategoriesAsync(Guid musicPatternId);
        Task<List<Category>> GetSelectedPatternCategoriesAsync(Guid stationId, string patternName);
    }
}
