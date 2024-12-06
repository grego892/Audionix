using SharedLibrary.Data;
using SharedLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SharedLibrary.Repositories;


namespace AudionixAudioServer.Repositories
{
    public class ProgramLogRepository : IProgramLogRepository
    {
        private readonly IDbContextFactory _dbContextFactory;

        public ProgramLogRepository(IDbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<ProgramLogItem> GetProgramLogItemAsync(Guid stationId, int nextPlayId, DateOnly? nextPlayDate)
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.Log
                .Where(log => log.StationId == stationId && log.Date == nextPlayDate && log.LogOrderID == nextPlayId)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> HasLogEntriesAsync(Guid stationId)
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.Log.AnyAsync(li => li.StationId == stationId);
        }

        public async Task AddProgramLogItemAsync(ProgramLogItem logItem)
        {
            using var context = _dbContextFactory.CreateDbContext();
            await context.Log.AddAsync(logItem);
            await context.SaveChangesAsync();
        }

        public async Task RemoveProgramLogItemAsync(ProgramLogItem logItem)
        {
            using var context = _dbContextFactory.CreateDbContext();
            context.Log.Remove(logItem);
            await context.SaveChangesAsync();
        }

        public async Task AddNewDayLogToDbLogAsync(List<ProgramLogItem> newDaysLog)
        {
            if (newDaysLog != null && newDaysLog.Count > 0)
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
            else
            {
                Log.Error("+++ AppDatabaseService - AddNewDayLogToDbLogAsync() -- NewDaysLog is null or empty.");
            }
        }

        public async Task ShiftLogItemsDownAsync(Guid stationId, int startIndex)
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

        public async Task ShiftLogItemsUpAsync(Guid stationId, int startIndex)
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

        public async Task AdvanceLogNextPlayAsync(Guid stationId)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var station = await context.Stations.FindAsync(stationId);
            if (station == null) return;

            var currentPlayingLogItem = await context.Log
                .FirstOrDefaultAsync(log => log.StationId == stationId && log.LogOrderID == station.CurrentPlayingId && log.Date == station.CurrentPlayingDate);

            if (currentPlayingLogItem == null) return;

            // Order logs by Date and LogOrderID, then find the next log item after the current playing log item
            var nextLogItem = await context.Log
                .Where(log => log.StationId == stationId &&
                              (log.Date > currentPlayingLogItem.Date ||
                              (log.Date == currentPlayingLogItem.Date && log.LogOrderID > currentPlayingLogItem.LogOrderID)))
                .OrderBy(log => log.Date)
                .ThenBy(log => log.LogOrderID)
                .FirstOrDefaultAsync();

            if (nextLogItem != null)
            {
                station.NextPlayId = nextLogItem.LogOrderID;
                station.NextPlayDate = nextLogItem.Date;
                context.Stations.Update(station);
                await context.SaveChangesAsync();
            }
        
        }

        public async Task CopyNextPlayToCurrentPlayingAsync(Guid stationId)
        {
            using var context = _dbContextFactory.CreateDbContext();
            var station = await context.Stations.FindAsync(stationId);
            if (station == null) return;
            station.CurrentPlayingId = station.NextPlayId;
            station.CurrentPlayingDate = station.NextPlayDate;
            await context.SaveChangesAsync();
        }
    }
}
