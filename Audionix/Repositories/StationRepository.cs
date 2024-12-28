using SharedLibrary.Data;
using SharedLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;
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
            try
            {
                using var context = _dbContextFactory.CreateDbContext();
                return await context.Stations.ToListAsync();
            }
            catch (Npgsql.NpgsqlException ex)
            {
                Log.Error(ex, "++++++ StationRepository.cs - PostgreSQL is not running or not set up correctly.");
                return new List<Station>();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "++++++ StationRepository.cs - An error occurred while getting stations.");
                return new List<Station>();
            }
        }

        public async Task<Station?> GetStationByIdAsync(Guid stationId)
        {
            try
            {
                using var context = _dbContextFactory.CreateDbContext();
                return await context.Stations
                                    .Where(s => s.StationId == stationId)
                                    .OrderBy(s => s.StationId)
                                    .FirstOrDefaultAsync();
            }
            catch (Npgsql.NpgsqlException ex)
            {
                Log.Error(ex, "++++++ StationRepository.cs - PostgreSQL is not running or not set up correctly.");
                return null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "++++++ StationRepository.cs - An error occurred while getting station by ID.");
                return null;
            }
        }

        public async Task AddStationAsync(Station station)
        {
            try
            {
                using var context = _dbContextFactory.CreateDbContext();
                await context.Stations.AddAsync(station);
                await context.SaveChangesAsync();
            }
            catch (Npgsql.NpgsqlException ex)
            {
                Log.Error(ex, "++++++ StationRepository.cs - PostgreSQL is not running or not set up correctly.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "++++++ StationRepository.cs - An error occurred while adding a station.");
            }
        }

        public async Task UpdateStationAsync(Station station)
        {
            try
            {
                using var context = _dbContextFactory.CreateDbContext();
                context.Stations.Update(station);
                await context.SaveChangesAsync();
            }
            catch (Npgsql.NpgsqlException ex)
            {
                Log.Error(ex, "++++++ StationRepository.cs - PostgreSQL is not running or not set up correctly.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "++++++ StationRepository.cs - An error occurred while updating a station.");
            }
        }

        public async Task DeleteStationAsync(Guid stationId)
        {
            try
            {
                using var context = _dbContextFactory.CreateDbContext();
                var station = await context.Stations.FindAsync(stationId);
                if (station != null)
                {
                    context.Stations.Remove(station);
                    await context.SaveChangesAsync();
                }
            }
            catch (Npgsql.NpgsqlException ex)
            {
                Log.Error(ex, "++++++ StationRepository.cs - PostgreSQL is not running or not set up correctly.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "++++++ StationRepository.cs - An error occurred while deleting a station.");
            }
        }

        public async Task<List<Folder>> GetFoldersForStationAsync(Guid stationId)
        {
            try
            {
                using var context = _dbContextFactory.CreateDbContext();
                return await context.Folders.Where(f => f.StationId == stationId).ToListAsync();
            }
            catch (Npgsql.NpgsqlException ex)
            {
                Log.Error(ex, "++++++ StationRepository.cs - PostgreSQL is not running or not set up correctly.");
                return new List<Folder>();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "++++++ StationRepository.cs - An error occurred while getting folders for station.");
                return new List<Folder>();
            }
        }

        public async Task UpdateStationNextPlayAsync(Guid stationId, int logOrderID, DateOnly Date)
        {
            try
            {
                using var context = _dbContextFactory.CreateDbContext();
                var station = await context.Stations.FirstOrDefaultAsync(s => s.StationId == stationId);
                if (station != null)
                {
                    station.NextPlayId = logOrderID;
                    station.NextPlayDate = Date;
                    context.Stations.Update(station);
                    await context.SaveChangesAsync();
                }
            }
            catch (Npgsql.NpgsqlException ex)
            {
                Log.Error(ex, "++++++ StationRepository.cs - PostgreSQL is not running or not set up correctly.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "++++++ StationRepository.cs - An error occurred while updating station next play.");
            }
        }
    }
}
