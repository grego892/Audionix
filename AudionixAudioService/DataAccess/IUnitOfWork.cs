using SharedLibrary.Models;
using AudionixAudioServer.Repositories;


namespace AudionixAudioServer.DataAccess
{
    public interface IUnitOfWork : IDisposable
    {
        IStationRepository Stations { get; }
        Task<int> CompleteAsync();
        Task<AppSettings> GetAppSettingsDataPathAsync();

    }
}
