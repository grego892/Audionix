using SharedLibrary.Models.MusicSchedule;

namespace SharedLibrary.Repositories
{
    public interface ISongCategoryRepository
    {
        Task<List<SongCategory>> GetSongCategoriesAsync(Guid stationId);
        Task AddSongCategoryAsync(SongCategory songCategory);
        Task DeleteSongCategoryAsync(int categoryId);
        Task<SongCategory?> GetSongCategoryByIdAsync(int songCategoryId);
        Task<List<string>> GetSongCategoryNamesAsync();
    }
}
