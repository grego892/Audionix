    using SharedLibrary.Models.MusicSchedule.Rules;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    namespace SharedLibrary.Repositories
    {
        public interface ISongScheduleRepository
        {
            Task<List<Category>> GetAllCategoriesAsync();
            Task<List<SoundCode>> GetAllSoundCodesAsync();
            Task<List<EnergyLevel>> GetAllEnergyLevelsAsync();
            Task AddCategoryAsync(Category category);
            Task RemoveCategoryAsync(Category category);
            Task AddSoundCodeAsync(SoundCode soundcode);
            Task RemoveSoundCodeAsync(SoundCode soundcode);
            Task<SongScheduleSettings> GetSongScheduleSettingsAsync();
            Task UpdateSongScheduleSettingsAsync(SongScheduleSettings settings);
            Task<List<Category>> GetCategoriesAsync(Guid stationId);
        }
    }
