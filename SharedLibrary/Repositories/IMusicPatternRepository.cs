using SharedLibrary.Models.MusicSchedule;

public interface IMusicPatternRepository
{
    Task<List<MusicPattern>> GetMusicPatternsAsync(int stationId);
    Task AddMusicPatternAsync(MusicPattern musicPattern);
    Task<MusicPattern?> GetMusicPatternByNameAsync(string name);
    Task DeleteMusicPatternAsync(MusicPattern musicPattern);
    Task AddSongCategoryToPatternAsync(int musicPatternId, int? categoryId);
    Task RemoveSongCategoryFromPatternAsync(MusicPattern musicPattern, PatternCategory patternCategory);
    Task MoveSongCategoryUpAsync(MusicPattern musicPattern, PatternCategory patternCategory);
    Task MoveSongCategoryDownAsync(MusicPattern musicPattern, PatternCategory patternCategory);
    Task<List<PatternCategory>> GetSelectedPatternCategoriesAsync(int musicPatternId);
    Task<List<PatternCategory>> GetSelectedPatternCategoriesAsync(int stationId, string patternName);
    Task<List<string>> GetMusicPatternNamesAsync();
    Task<List<int>> GetMusicPatternsForDayAsync(int stationId, DayOfWeek day);
    Task<List<MusicGridItem>> GetMusicGridItemsAsync(int stationId);
    Task AddMusicGridItemAsync(MusicGridItem item);
    Task UpdateMusicGridItemAsync(MusicGridItem musicGridItem);
    Task<int> GetNextPatternIdAsync();
}

