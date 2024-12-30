using SharedLibrary.Models;

namespace SharedLibrary.Repositories
{
    public interface IProgramLogRepository
    {
        Task<List<ProgramLogItem>> GetProgramLogItemsAsync(Guid stationId, DateOnly? nextPlayDate);
        Task<List<ProgramLogItem>> GetProgramLogItemsAsync(Guid stationId, int nextPlayId, DateOnly? nextPlayDate);
        Task<List<ProgramLogItem>> GetFullProgramLogForStationAsync(Guid stationId);
        Task<bool> HasLogEntriesAsync(Guid stationId);
        Task AddProgramLogItemAsync(ProgramLogItem logItem);
        Task RemoveProgramLogItemAsync(ProgramLogItem logItem);
        Task AddNewDayLogToDbLogAsync(List<ProgramLogItem> newDaysLog);
        Task ShiftLogItemsDownAsync(Guid stationId, int startIndex);
        Task ShiftLogItemsUpAsync(Guid stationId, int startIndex);
        Task<ProgramLogItem> GetProgramLogItemAsync(Guid stationId, int nextPlayId, DateOnly? nextPlayDate);
        Task AdvanceLogNextPlayAsync(Guid stationId);
    }
}
