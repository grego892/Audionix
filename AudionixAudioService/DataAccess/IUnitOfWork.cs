using AudionixAudioServer.Models;
using AudionixAudioServer.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudionixAudioServer.DataAccess
{
    public interface IUnitOfWork : IDisposable
    {
        IStationRepository Stations { get; }
        Task<int> CompleteAsync();
        Task<AppSettings> GetAppSettingsDataPathAsync();

    }
}
