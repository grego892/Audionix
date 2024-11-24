using Audionix.Models;
using Audionix.Repositories;


namespace Audionix.DataAccess
{
    public interface IUnitOfWork : IDisposable
    {
        IStationRepository Stations { get; }
        Task<int> CompleteAsync();
        Task<AppSettings> GetAppSettingsDataPathAsync();
    }
}
