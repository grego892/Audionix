using SharedLibrary.Models.MusicSchedule;
using SharedLibrary.Models;

public interface IStationRepository
{
    Task<List<Station>> GetStationsAsync();
    Task<Station?> GetStationByIdAsync(Guid stationId);
    Task AddStationAsync(Station station);
    Task UpdateStationAsync(Station station);
    Task DeleteStationAsync(Guid stationId);
    Task<List<Folder>> GetFoldersForStationAsync(Guid stationId);
    Task UpdateStationNextPlayAsync(Guid stationId, int logOrderID, DateOnly Date);
    Task<List<SongCategory>> GetSongCategoriesAsync(Guid stationId);
    Task AddSongCategoryAsync(SongCategory songCategory);
    Task DeleteSongCategoryAsync(int songCategoryId);
    Task<List<Guid>> GetMusicPatternsForDayAsync(Guid stationId, DayOfWeek day);
    Task<List<SongCategory>> GetSongCategoriesForPatternsAsync(List<int> musicPatterns);
    Task<List<AudioMetadata>> GetScheduledSongsAsync(List<SongCategory> categories, Dictionary<string, int> songCategoryRotationIndex);
    Task AddNewDayLogToDbLogAsync(List<ProgramLogItem> newDaysLog);
    Task<List<MusicGridItem>> GetMusicGridItemsAsync(Guid stationId);
    Task AddMusicGridItemAsync(MusicGridItem item);
    Task<List<MusicPattern>> GetMusicPatternsAsync(Guid stationId);
    Task<List<AudioMetadata>> GetAudioFilesAsync();
    Task<AudioMetadata?> GetAudioFileByIdAsync(int id);
    Task AddAudioFileAsync(AudioMetadata audioMetadata);
    Task DeleteAudioFileAsync(AudioMetadata audioMetadata);
    Task<Folder?> GetFolderByIdAsync(int id);
    Task AddFolderAsync(Folder folder);
    Task DeleteFolderAsync(Folder folder);
    Task UpdateAudioMetadataAsync(AudioMetadata audioMetadata);
    Task UpdateMusicGridItemAsync(MusicGridItem musicGridItem);
    Task<AppSettings?> GetAppSettingsDataPathAsync(); // Updated to nullable
    Task<AudioMetadata?> GetAudioFileByFilenameAsync(string filename); // Updated to nullable
    Task UpdateProgramLogItemAsync(ProgramLogItem logItem);
}
