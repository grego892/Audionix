using Audionix.Data;
using Audionix.DataAccess;
using Audionix.Models;
using Audionix.Repositories;
using Microsoft.EntityFrameworkCore;


namespace DataAccess.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public IStationRepository Stations { get; }

        public UnitOfWork(ApplicationDbContext context, IStationRepository stationRepository)
        {
            _context = context;
            Stations = stationRepository;
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }
        public async Task<AppSettings> GetAppSettingsDataPathAsync()
        {
            return await _context.AppSettings.FirstOrDefaultAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
