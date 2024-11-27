using Audionix.Models;

namespace Audionix.Repositories
{
    public interface IProgramLogRepository
    {
        Task<List<ProgramLogItem>> GetProgramLogItemsAsync(Guid stationId);
        Task<bool> HasLogEntriesAsync(Guid stationId);
        Task AddProgramLogItemAsync(ProgramLogItem logItem);
        Task RemoveProgramLogItemAsync(ProgramLogItem logItem);
        Task AddNewDayLogToDbLogAsync(List<ProgramLogItem> newDaysLog);
    }
}
