using SharedLibrary.Models;
using SharedLibrary.Models.MusicSchedule;
using System.Threading.Tasks;

namespace SharedLibrary.Repositories
{
    public interface IStationRepository
    {
        Task<List<Station>> GetStationsAsync();
        Task<Station?> GetStationByIdAsync(int stationId);
        Task AddStationAsync(Station station);
        Task UpdateStationAsync(Station station);
        Task DeleteStationAsync(int stationId);
        Task<List<Folder>> GetFoldersForStationAsync(int stationId);
        Task UpdateStationNextPlayAsync(int stationId, int logOrderID, DateOnly Date);
        Task<List<Category>> GetSongCategoriesAsync(int stationId);
        Task AddSongCategoryAsync(Category songCategory);
        Task DeleteSongCategoryAsync(int songCategoryId);
        Task<List<int>> GetMusicPatternsForDayAsync(int stationId, DayOfWeek day);
        Task<List<Category>> GetSongCategoriesForPatternsAsync(List<int> musicPatterns);
        Task<List<AudioMetadata>> GetScheduledSongsAsync(List<Category> categories, Dictionary<string, int> songCategoryRotationIndex);
        Task AddNewDayLogToDbLogAsync(List<ProgramLogItem> newDaysLog);
        Task<List<MusicGridItem>> GetMusicGridItemsAsync(int stationId);
        Task AddMusicGridItemAsync(MusicGridItem item);
        Task<List<MusicPattern>> GetMusicPatternsAsync(int stationId);
        Task<List<AudioMetadata>> GetAudioFilesAsync();
        Task<AudioMetadata?> GetAudioFileByIdAsync(int id);
        Task AddAudioFileAsync(AudioMetadata audioMetadata);
        Task DeleteAudioFileAsync(AudioMetadata audioMetadata);
        Task<Folder?> GetFolderByIdAsync(int id);
        Task AddFolderAsync(Folder folder);
        Task DeleteFolderAsync(Folder folder);
        Task UpdateAudioMetadataAsync(AudioMetadata audioMetadata);
        Task UpdateMusicGridItemAsync(MusicGridItem musicGridItem);
        Task<AppSettings> GetAppSettingsDataPathAsync();
        Task<AudioMetadata> GetAudioFileByFilenameAsync(string filename);
        //Task<ProgramLogItem?> GetProgramLogItemAsync(int stationId, int logOrderID);
        Task UpdateProgramLogItemAsync(ProgramLogItem logItem);

    }
}
