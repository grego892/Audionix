using SharedLibrary.Models;

namespace Audionix.Repositories
{
    public interface IFolderRepository
    {
        Task<List<Folder>> GetFoldersForStationAsync(Guid stationId);
        Task<Folder?> GetFolderByIdAsync(int id);
        Task AddFolderAsync(Folder folder);
        Task DeleteFolderAsync(Folder folder);
    }
}
