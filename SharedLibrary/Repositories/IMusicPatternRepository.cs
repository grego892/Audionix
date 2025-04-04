using SharedLibrary.Models.MusicSchedule;

namespace SharedLibrary.Repositories
{
    public interface IMusicPatternRepository
    {
        Task<List<MusicPattern>> GetMusicPatternsAsync(int stationId);
        Task AddMusicPatternAsync(MusicPattern musicPattern);
        Task<MusicPattern?> GetMusicPatternByNameAsync(string name);
        Task DeleteMusicPatternAsync(MusicPattern musicPattern);
        //Task AddSongCategoryToPatternAsync(MusicPattern musicPattern, SongCategory songCategory);
        Task AddSongCategoryToPatternAsync(int musicPatternId);
        Task RemoveSongCategoryFromPatternAsync(MusicPattern musicPattern, SongCategory songCategory);
        Task MoveSongCategoryUpAsync(MusicPattern musicPattern, SongCategory songCategory);
        Task MoveSongCategoryDownAsync(MusicPattern musicPattern, SongCategory songCategory);
        Task<List<SongCategory>> GetSelectedPatternCategoriesAsync(int musicPatternId);
        Task<List<SongCategory>> GetSelectedPatternCategoriesAsync(int stationId, string patternName);
        Task<List<string>> GetMusicPatternNamesAsync();
        Task<List<int>> GetMusicPatternsForDayAsync(int stationId, DayOfWeek day);
        Task<List<MusicGridItem>> GetMusicGridItemsAsync(int stationId);
        Task AddMusicGridItemAsync(MusicGridItem item);
        Task UpdateMusicGridItemAsync(MusicGridItem musicGridItem);
        Task<int> GetNextPatternIdAsync();
    }
}
