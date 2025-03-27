using SharedLibrary.Models.MusicSchedule;

namespace SharedLibrary.Repositories
{
    public interface IMusicPatternRepository
    {
        Task<List<MusicPattern>> GetMusicPatternsAsync(Guid stationId);
        Task AddMusicPatternAsync(MusicPattern musicPattern);
        Task<MusicPattern?> GetMusicPatternByNameAsync(string name);
        Task DeleteMusicPatternAsync(MusicPattern musicPattern);
        Task AddSongCategoryToPatternAsync(int musicPatternId, int songCategoryId);
        Task RemoveSongCategoryFromPatternAsync(MusicPattern musicPattern, SongCategory songCategory);
        Task MoveSongCategoryUpAsync(MusicPattern musicPattern, SongCategory songCategory);
        Task MoveSongCategoryDownAsync(MusicPattern musicPattern, SongCategory songCategory);
        Task<List<SongCategory>> GetSelectedPatternCategoriesAsync(int musicPatternId);
        Task<List<SongCategory>> GetSelectedPatternCategoriesAsync(Guid stationId, string patternName);
        Task<List<string>> GetMusicPatternNamesAsync();
        Task<List<Guid>> GetMusicPatternsForDayAsync(Guid stationId, DayOfWeek day);
        Task<List<MusicGridItem>> GetMusicGridItemsAsync(Guid stationId);
        Task AddMusicGridItemAsync(MusicGridItem item);
        Task UpdateMusicGridItemAsync(MusicGridItem musicGridItem);
    }
}
