using Audionix.Data;
using Audionix.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Audionix.Repositories
{
    public class ProgramLogRepository : IProgramLogRepository
    {
        private readonly IDbContextFactory _dbContextFactory;

        public ProgramLogRepository(IDbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<List<ProgramLogItem>> GetProgramLogItemsAsync(Guid stationId)
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.Log
                .Where(log => log.StationId == stationId)
                .OrderBy(log => log.LogOrderID)
                .ToListAsync();
        }

        public async Task<List<ProgramLogItem>> GetProgramLogItemsAsync(Guid stationId, DateOnly logDate)
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.Log
                .Where(log => log.StationId == stationId && log.Date == logDate)
                .OrderBy(log => log.LogOrderID)
                .ToListAsync();
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
            using var context = _dbContextFactory.CreateDbContext();
            if (newDaysLog == null || !newDaysLog.Any())
            {
                Log.Error("+++ AppDatabaseService - AddNewDayLogToDbLogAsync() -- NewDaysLog is null or empty.");
                return;
            }

            var newLogDate = newDaysLog.First().Date;

            int maxLogOrderIDForPreviousDate = await context.Log
                .AsNoTracking()
                .Where(log => log.Date < newLogDate)
                .MaxAsync(log => (int?)log.LogOrderID) ?? 0;

            int currentLogOrderID = maxLogOrderIDForPreviousDate;
            foreach (var logItem in newDaysLog)
            {
                logItem.LogOrderID = ++currentLogOrderID;
            }

            await context.Log.AddRangeAsync(newDaysLog);
            await context.SaveChangesAsync();

            var logsToRenumber = await context.Log
                .Where(log => log.Date > newLogDate)
                .OrderBy(log => log.Date)
                .ThenBy(log => log.LogOrderID)
                .ToListAsync();

            currentLogOrderID = newDaysLog.Max(log => log.LogOrderID);
            foreach (var logItem in logsToRenumber)
            {
                logItem.LogOrderID = ++currentLogOrderID;
            }

            context.Log.UpdateRange(logsToRenumber);
            await context.SaveChangesAsync();
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
    }
}
