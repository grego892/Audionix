using SharedLibrary.Models;

namespace Audionix.Repositories
{
    public interface IAppSettingsRepository
    {
        Task<AppSettings?> GetAppSettingsAsync();
        Task SaveAppSettingsAsync(AppSettings appSettings);
    }
}
