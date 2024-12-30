using SharedLibrary.Data;
using SharedLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace SharedLibrary.Repositories
{
    public class ProgramLogRepository : IProgramLogRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public ProgramLogRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<ProgramLogItem> GetProgramLogItemAsync(Guid stationId, int nextPlayId, DateOnly? nextPlayDate)
        {
            try
            {
                using var context = _dbContextFactory.CreateDbContext();
                return await context.Log
                    .Where(log => log.StationId == stationId && log.Date == nextPlayDate && log.LogOrderID == nextPlayId)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetProgramLogItemAsync");
                throw;
            }
        }

        public async Task<List<ProgramLogItem>> GetFullProgramLogForStationAsync(Guid stationId)
        {
            try
            {
                using var context = _dbContextFactory.CreateDbContext();
                return await context.Log
                    .Where(log => log.StationId == stationId)
                    .OrderBy(log => log.LogOrderID)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetFullProgramLogForStationAsync");
                throw;
            }
        }

        public async Task<List<ProgramLogItem>> GetProgramLogItemsAsync(Guid stationId, DateOnly? date)
        {
            try
            {
                using var context = _dbContextFactory.CreateDbContext();
                return await context.Log
                    .Where(log => log.StationId == stationId && log.Date == date)
                    .OrderBy(log => log.LogOrderID)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetProgramLogItemsAsync");
                throw;
            }
        }

        public async Task<List<ProgramLogItem>> GetProgramLogItemsAsync(Guid stationId, int nextPlayId, DateOnly? nextPlayDate)
        {
            try
            {
                using var context = _dbContextFactory.CreateDbContext();
                return await context.Log
                    .Where(log => log.StationId == stationId && log.Date == nextPlayDate)
                    .OrderBy(log => log.LogOrderID)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetProgramLogItemsAsync");
                throw;
            }
        }

        public async Task<bool> HasLogEntriesAsync(Guid stationId)
        {
            try
            {
                using var context = _dbContextFactory.CreateDbContext();
                return await context.Log.AnyAsync(li => li.StationId == stationId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in HasLogEntriesAsync");
                throw;
            }
        }

        public async Task AddProgramLogItemAsync(ProgramLogItem logItem)
        {
            try
            {
                using var context = _dbContextFactory.CreateDbContext();
                await context.Log.AddAsync(logItem);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in AddProgramLogItemAsync");
                throw;
            }
        }

        public async Task RemoveProgramLogItemAsync(ProgramLogItem logItem)
        {
            try
            {
                using var context = _dbContextFactory.CreateDbContext();
                context.Log.Remove(logItem);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in RemoveProgramLogItemAsync");
                throw;
            }
        }

        public async Task AddNewDayLogToDbLogAsync(List<ProgramLogItem> newDaysLog)
        {
            if (newDaysLog == null || newDaysLog.Count == 0)
            {
                Log.Error("+++ ProgramLogRepository - AddNewDayLogToDbLogAsync() -- NewDaysLog is null or empty.");
                return;
            }

            try
            {
                using var context = _dbContextFactory.CreateDbContext();
                var newLogDate = newDaysLog.First().Date;

                // Assign LogOrderID to the new log items starting from 1
                int currentLogOrderID = 0;
                foreach (var logItem in newDaysLog)
                {
                    logItem.LogOrderID = ++currentLogOrderID;
                }

                // Insert the new log items into the database
                await context.Log.AddRangeAsync(newDaysLog);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in AddNewDayLogToDbLogAsync");
                throw;
            }
        }

        public async Task ShiftLogItemsDownAsync(Guid stationId, int startIndex)
        {
            try
            {
                using var context = _dbContextFactory.CreateDbContext();
                var itemsToShift = await context.Log
                    .Where(log => log.StationId == stationId && log.LogOrderID >= startIndex)
                    .OrderBy(log => log.LogOrderID)
                    .ToListAsync();

                foreach (var item in itemsToShift)
                {
                    item.LogOrderID++;
                }

                context.Log.UpdateRange(itemsToShift);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in ShiftLogItemsDownAsync");
                throw;
            }
        }

        public async Task ShiftLogItemsUpAsync(Guid stationId, int startIndex)
        {
            try
            {
                using var context = _dbContextFactory.CreateDbContext();
                var itemsToShift = await context.Log
                    .Where(log => log.StationId == stationId && log.LogOrderID > startIndex)
                    .OrderBy(log => log.LogOrderID)
                    .ToListAsync();

                foreach (var item in itemsToShift)
                {
                    item.LogOrderID--;
                }

                context.Log.UpdateRange(itemsToShift);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in ShiftLogItemsUpAsync");
                throw;
            }
        }

        public async Task AdvanceLogNextPlayAsync(Guid stationId)
        {
            Log.Debug($"--- ProgramLogRepository - AdvanceLogNextPlayAsync() -- Method Starting.   StationId: {stationId}");
            try
            {
                using var context = _dbContextFactory.CreateDbContext();
                var station = await context.Stations.FindAsync(stationId);
                if (station == null)
                {
                    Log.Error($"--- ProgramLogRepository - AdvanceLogNextPlayAsync() -- Station not found.   StationId: {stationId}");
                    return;
                }

                Log.Debug($"--- ProgramLogRepository - AdvanceLogNextPlayAsync() -- Station found.   StationId: {stationId}");

                // Update the station's current playing log item
                station.CurrentPlayingId = station.NextPlayId;
                station.CurrentPlayingDate = station.NextPlayDate;

                Log.Debug($"--- ProgramLogRepository - AdvanceLogNextPlayAsync() -- CurrentPlayingId: {station.CurrentPlayingId}, CurrentPlayingDate: {station.CurrentPlayingDate}");

                var currentPlayingLogItem = await context.Log
                    .FirstOrDefaultAsync(log => log.StationId == stationId && log.LogOrderID == station.CurrentPlayingId && log.Date == station.CurrentPlayingDate);

                if (currentPlayingLogItem == null)
                {
                    Log.Error($"--- ProgramLogRepository - AdvanceLogNextPlayAsync() -- CurrentPlayingLogItem is null.   StationId: {stationId}, LogOrderID: {station.CurrentPlayingId}, Date: {station.CurrentPlayingDate}");
                    return;
                }

                Log.Debug($"--- ProgramLogRepository - AdvanceLogNextPlayAsync() -- CurrentPlayingLogItem found.   CurrentPlayingLogItem: ID: {currentPlayingLogItem.StationId} - {currentPlayingLogItem.Date} - {currentPlayingLogItem.Title} - {currentPlayingLogItem.Artist}");

                // Find the next log item
                var nextLogItem = await context.Log
                    .Where(log => log.StationId == stationId &&
                                  (log.Date > currentPlayingLogItem.Date ||
                                  (log.Date == currentPlayingLogItem.Date && log.LogOrderID > currentPlayingLogItem.LogOrderID)))
                    .OrderBy(log => log.Date)
                    .ThenBy(log => log.LogOrderID)
                    .FirstOrDefaultAsync();

                Log.Debug($"--- ProgramLogRepository - AdvanceLogNextPlayAsync() -- NextLogItem found.   NextLogItem: {nextLogItem?.LogOrderID} - {nextLogItem?.Date} - {nextLogItem?.Title} - {nextLogItem?.Artist}");

                if (nextLogItem != null)
                {
                    station.NextPlayId = nextLogItem.LogOrderID;
                    station.NextPlayDate = nextLogItem.Date;
                }
                else
                {
                    // If no next log item is found, reset the next play fields
                    station.NextPlayId = 0;
                    station.NextPlayDate = null;
                }

                context.Stations.Update(station);
                await context.SaveChangesAsync();

                Log.Debug($"--- ProgramLogRepository - AdvanceLogNextPlayAsync() -- Method Ending.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in AdvanceLogNextPlayAsync");
                throw;
            }
        }
    }
}
