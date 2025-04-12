using SharedLibrary.Models.MusicSchedule;

namespace SharedLibrary.Repositories
{
    public interface ISongCategoryRepository
    {
        Task<List<Category>> GetSongCategoriesAsync(int stationId);
        Task AddSongCategoryAsync(Category songCategory);
        Task DeleteSongCategoryAsync(int categoryId);
        Task<Category?> GetSongCategoryByIdAsync(int songCategoryId);
        Task<List<string>> GetSongCategoryNamesAsync();
        Task<List<Category>> GetSongCategoriesForPatternsAsync(List<int> musicPatterns);
    }
}
