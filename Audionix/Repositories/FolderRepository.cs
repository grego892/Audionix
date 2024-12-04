using SharedLibrary.Data;
using SharedLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace Audionix.Repositories
{
    public class FolderRepository : IFolderRepository
    {
        private readonly IDbContextFactory _dbContextFactory;

        public FolderRepository(IDbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<List<Folder>> GetFoldersForStationAsync(Guid stationId)
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.Folders.Where(f => f.StationId == stationId).ToListAsync();
        }

        public async Task<Folder?> GetFolderByIdAsync(int id)
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.Folders.FindAsync(id);
        }

        public async Task AddFolderAsync(Folder folder)
        {
            using var context = _dbContextFactory.CreateDbContext();
            await context.Folders.AddAsync(folder);
            await context.SaveChangesAsync();
        }

        public async Task DeleteFolderAsync(Folder folder)
        {
            using var context = _dbContextFactory.CreateDbContext();
            context.Folders.Remove(folder);
            await context.SaveChangesAsync();
        }
    }
}
