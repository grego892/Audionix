using SharedLibrary.Models.MusicSchedule;
using SharedLibrary.Models.MusicSchedule.Rules;

public interface ISongScheduleRepository
{
    Task<List<Category>> GetAllCategoriesAsync();
    Task<List<SoundCode>> GetAllSoundCodesAsync();
    Task<List<EnergyLevel>> GetAllEnergyLevelsAsync();
    Task AddCategoryAsync(Category category);
    Task AddEnergyLevelAsync(EnergyLevel energyLevel);
    Task RemoveCategoryAsync(Category category);
    Task AddSoundCodeAsync(SoundCode soundcode);
    Task RemoveSoundCodeAsync(SoundCode soundcode);
    Task<SongScheduleSettings?> GetSongScheduleSettingsAsync(); // Updated to return nullable
    Task UpdateSongScheduleSettingsAsync(SongScheduleSettings settings);
    Task<List<Category>> GetCategoriesAsync(int stationId);
    Task<List<SoundCode>> GetSoundCodesAsync(int stationId); // Added method signature
    Task<List<EnergyLevel>> GetEnergyLevelsAsync(int stationId); // Added method signature
}
