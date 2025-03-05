using Microsoft.EntityFrameworkCore;
using SharedLibrary.Data;
using SharedLibrary.Models.MusicSchedule.Rules;
using System.Collections.Generic;
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

        public async Task<SongScheduleSettings> GetSongScheduleSettingsAsync()
        {
            // Assuming there is only one SongScheduleSettings record
            return await _context.SongScheduleSettings.FirstOrDefaultAsync();
        }

        public async Task UpdateSongScheduleSettingsAsync(SongScheduleSettings settings)
        {
            _context.SongScheduleSettings.Update(settings);
            await _context.SaveChangesAsync();
        }
    }
}
