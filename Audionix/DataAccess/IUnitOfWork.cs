using Audionix.Repositories;
using SharedLibrary.Models;

public interface IUnitOfWork : IDisposable
{
    IStationRepository Stations { get; }
    Task<int> CompleteAsync();
    Task<AppSettings?> GetAppSettingsDataPathAsync();
}
