using SharedLibrary.Models;

namespace SharedLibrary.Repositories
{
    public interface IProgramLogRepository
    {
        Task<List<ProgramLogItem>> GetProgramLogItemsAsync(int stationId, DateOnly? nextPlayDate);
        Task<List<ProgramLogItem>> GetProgramLogItemsAsync(int stationId, int nextPlayId, DateOnly? nextPlayDate);
        Task<List<ProgramLogItem>> GetFullProgramLogForStationAsync(int stationId);
        Task<bool> HasLogEntriesAsync(int stationId);
        Task AddProgramLogItemAsync(ProgramLogItem logItem);
        Task RemoveProgramLogItemAsync(ProgramLogItem logItem);
        Task AddNewDayLogToDbLogAsync(List<ProgramLogItem> newDaysLog);
        Task ShiftLogItemsDownAsync(int stationId, int startIndex);
        Task ShiftLogItemsUpAsync(int stationId, int startIndex);
        Task<ProgramLogItem> GetProgramLogItemAsync(int stationId, int nextPlayId, DateOnly? nextPlayDate);
        Task AdvanceLogNextPlayAsync(int stationId);
        Task RemoveOlderDaysFromDbLogAsync(int daysBack);
    }
}
