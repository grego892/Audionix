using SharedLibrary.Data;
using SharedLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace SharedLibrary.Repositories
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

        public async Task<AppSettings?> GetAppSettingsDataPathAsync()
        {
            return await _context.AppSettings.FirstOrDefaultAsync();
        }

        public async Task<AudioMetadata?> GetAudioFileByFilenameAsync(string filename)
        {
            return await Stations.GetAudioFileByFilenameAsync(filename);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
