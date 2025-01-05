using SharedLibrary.Models.MusicSchedule;

namespace SharedLibrary.Repositories
{
    public interface ISongCategoryRepository
    {
        Task<List<SongCategory>> GetSongCategoriesAsync(Guid stationId);
        Task AddSongCategoryAsync(SongCategory songCategory);
        Task DeleteSongCategoryAsync(Guid categoryId);
        Task<SongCategory?> GetSongCategoryByIdAsync(Guid songCategoryId);
        Task<List<string>> GetSongCategoryNamesAsync();
        Task<List<SongCategory>> GetSongCategoriesForPatternsAsync(List<Guid> musicPatterns);
    }
}
