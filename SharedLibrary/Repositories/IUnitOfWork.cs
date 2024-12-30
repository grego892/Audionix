using SharedLibrary.Models;

namespace SharedLibrary.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IStationRepository Stations { get; }
        Task<int> CompleteAsync();
        Task<AppSettings?> GetAppSettingsDataPathAsync();
    }
}
