using Microsoft.EntityFrameworkCore;
using SharedLibrary.Data;
using SharedLibrary.Models.MusicSchedule.Rules;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharedLibrary.Repositories
{
    public class SongScheduleRepository : ISongScheduleRepository
    {
        private readonly ApplicationDbContext _context;

        public SongScheduleRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories.ToListAsync();
        }

        public async Task<List<SoundCode>> GetAllSoundCodesAsync()
        {
            return await _context.SoundCodes.ToListAsync();
        }

        public async Task<List<EnergyLevel>> GetAllEnergyLevelsAsync()
        {
            return await _context.EnergyLevels.ToListAsync();
        }

        public async Task AddCategoryAsync(Category category)
        {
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveCategoryAsync(Category category)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }

        public async Task AddSoundCodeAsync(SoundCode soundcode)
        {
            await _context.SoundCodes.AddAsync(soundcode);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveSoundCodeAsync(SoundCode soundcode)
        {
            _context.SoundCodes.Remove(soundcode);
            await _context.SaveChangesAsync();
        }

        public async Task<SongScheduleSettings?> GetSongScheduleSettingsAsync()
        {
            return await _context.SongScheduleSettings.FirstOrDefaultAsync();
        }

        public async Task UpdateSongScheduleSettingsAsync(SongScheduleSettings settings)
        {
            _context.SongScheduleSettings.Update(settings);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Category>> GetCategoriesAsync(int stationId)
        {
            return await _context.Categories.Where(c => c.StationId == stationId).ToListAsync();
        }

        public async Task<List<SoundCode>> GetSoundCodesAsync(int stationId)
        {
            return await _context.SoundCodes.Where(sc => sc.StationId == stationId).ToListAsync();
        }

        public async Task<List<EnergyLevel>> GetEnergyLevelsAsync(int stationId)
        {
            return await _context.EnergyLevels.Where(el => el.StationId == stationId).ToListAsync();
        }
    }
}
