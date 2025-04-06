using SharedLibrary.Models.MusicSchedule;
using SharedLibrary.Models.MusicSchedule.Rules;

public interface ISongScheduleRepository
{
    Task<List<SongCategory>> GetAllCategoriesAsync();
    Task<List<SoundCode>> GetAllSoundCodesAsync();
    Task<List<EnergyLevel>> GetAllEnergyLevelsAsync();
    Task AddCategoryAsync(SongCategory category);
    Task AddEnergyLevelAsync(EnergyLevel energyLevel);
    Task RemoveCategoryAsync(SongCategory category);
    Task AddSoundCodeAsync(SoundCode soundcode);
    Task RemoveSoundCodeAsync(SoundCode soundcode);
    Task<SongScheduleSettings?> GetSongScheduleSettingsAsync(); // Updated to return nullable
    Task UpdateSongScheduleSettingsAsync(SongScheduleSettings settings);
    Task<List<SongCategory>> GetCategoriesAsync(int stationId);
    Task<List<SoundCode>> GetSoundCodesAsync(int stationId); // Added method signature
    Task<List<EnergyLevel>> GetEnergyLevelsAsync(int stationId); // Added method signature
}
