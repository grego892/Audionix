using SharedLibrary.Models;

namespace SharedLibrary.Repositories
{
    public interface IAppSettingsRepository
    {
        Task<AppSettings?> GetAppSettingsAsync();
        Task SaveAppSettingsAsync(AppSettings appSettings);
    }
}
