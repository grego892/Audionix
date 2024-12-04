using SharedLibrary.Models;
using SharedLibrary.Models.MusicSchedule;
using System.Threading.Tasks;

namespace Audionix.Repositories
{
    public interface IStationRepository
    {
        // Station methods
        Task<List<Station>> GetStationsAsync();
        Task<Station?> GetStationByIdAsync(Guid stationId);
        Task AddStationAsync(Station station);
        Task UpdateStationAsync(Station station);
        Task DeleteStationAsync(Guid stationId);
        Task<List<Folder>> GetFoldersForStationAsync(Guid stationId);
        Task UpdateStationNextPlayAsync(Guid stationId, int logOrderID, DateOnly Date);
    }
}
