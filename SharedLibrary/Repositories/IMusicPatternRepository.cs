using SharedLibrary.Models.MusicSchedule;

namespace SharedLibrary.Repositories
{
    public interface IMusicPatternRepository
    {
        Task<List<MusicPattern>> GetMusicPatternsAsync(Guid stationId);
        Task AddMusicPatternAsync(MusicPattern musicPattern);
        Task<MusicPattern?> GetMusicPatternByNameAsync(string name);
        Task DeleteMusicPatternAsync(MusicPattern musicPattern);
        //Task AddCategoryToPatternAsync(MusicPattern musicPattern, Category category);
        Task AddCategoryToPatternAsync(Guid musicPatternId, Guid categoryId);
        Task RemoveCategoryFromPatternAsync(MusicPattern musicPattern, Category category);
        Task MoveCategoryUpAsync(MusicPattern musicPattern, Category category);
        Task MoveCategoryDownAsync(MusicPattern musicPattern, Category category);
        Task<List<Category>> GetSelectedPatternCategoriesAsync(Guid musicPatternId);
        Task<List<Category>> GetSelectedPatternCategoriesAsync(Guid stationId, string patternName);
        Task<List<string>> GetMusicPatternNamesAsync();
        Task<List<Guid>> GetMusicPatternsForDayAsync(Guid stationId, DayOfWeek day);
        Task<List<MusicGridItem>> GetMusicGridItemsAsync(Guid stationId);
        Task AddMusicGridItemAsync(MusicGridItem item);
        Task UpdateMusicGridItemAsync(MusicGridItem musicGridItem);
    }
}
