using SharedLibrary.Models.MusicSchedule.Rules;

public interface ISongScheduleRepository
{
    Task<List<Category>> GetAllCategoriesAsync();
    Task<List<SoundCode>> GetAllSoundCodesAsync();
    Task<List<EnergyLevel>> GetAllEnergyLevelsAsync();
    Task AddCategoryAsync(Category category);
    Task RemoveCategoryAsync(Category category);
    Task AddSoundCodeAsync(SoundCode soundcode);
    Task RemoveSoundCodeAsync(SoundCode soundcode);
    Task<SongScheduleSettings?> GetSongScheduleSettingsAsync(); // Updated to return nullable
    Task UpdateSongScheduleSettingsAsync(SongScheduleSettings settings);
    Task<List<Category>> GetCategoriesAsync(Guid stationId);
    Task<List<SoundCode>> GetSoundCodesAsync(Guid stationId); // Added method signature
    Task<List<EnergyLevel>> GetEnergyLevelsAsync(Guid stationId); // Added method signature
}
