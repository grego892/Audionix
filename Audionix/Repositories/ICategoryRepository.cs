using Audionix.Models.MusicSchedule;

namespace Audionix.Repositories
{
    public interface ICategoryRepository
    {
        Task<List<Category>> GetCategoriesAsync(Guid stationId);
        Task AddCategoryAsync(Category category);
        Task DeleteCategoryAsync(Guid categoryId);
        Task<Category?> GetCategoryByIdAsync(Guid categoryId);
        Task<List<string>> GetCategoryNamesAsync();
        Task<List<Category>> GetCategoriesForPatternsAsync(List<Guid> musicPatterns);
    }
}
