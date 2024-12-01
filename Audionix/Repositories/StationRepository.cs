using Audionix.Data;
using Audionix.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using static ATL.Logging.Log;

namespace Audionix.Repositories
{
    public class StationRepository : IStationRepository
    {
        private readonly IDbContextFactory _dbContextFactory;

        public StationRepository(IDbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<List<Station>> GetStationsAsync()
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.Stations.ToListAsync();
        }

        public async Task<Station?> GetStationByIdAsync(Guid stationId)
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.Stations
                                .Where(s => s.StationId == stationId)
                                .OrderBy(s => s.StationId)
                                .FirstOrDefaultAsync();
        }


        public async Task AddStationAsync(Station station)
        {
            using var context = _dbContextFactory.CreateDbContext();
            await context.Stations.AddAsync(station);
            await context.SaveChangesAsync();
        }

        public async Task UpdateStationAsync(Station station)
        {
            using var context = _dbContextFactory.CreateDbContext();
            context.Stations.Update(station);
            await context.SaveChangesAsync();
        }

        public async Task DeleteStationAsync(Guid stationId)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var station = await context.Stations.FindAsync(stationId);
            if (station != null)
            {
                context.Stations.Remove(station);
                await context.SaveChangesAsync();
            }
        }
        public async Task<List<Folder>> GetFoldersForStationAsync(Guid stationId)
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.Folders.Where(f => f.StationId == stationId).ToListAsync();
        }
        public async Task UpdateStationNextPlayAsync(Guid stationId, int logOrderID)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var station = await context.Stations.FirstOrDefaultAsync(s => s.StationId == stationId);
            if (station != null)
            {
                station.NextPlay = logOrderID;
                context.Stations.Update(station);
                await context.SaveChangesAsync();
            }
        }
    }
}
