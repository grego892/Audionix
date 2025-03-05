using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Repositories;
using SharedLibrary.Models.MusicSchedule;
using SharedLibrary.Models.MusicSchedule.Rules;
using Audionix.Services;

namespace Audionix.Components.Pages.MusicSchedule
{
    public partial class Rules
    {
        private string? newCategory;
        private string? newSoundCode;
        private string? newEnergyLevel;
        private List<string> categories = new List<string>();
        private List<string> soundCodes = new List<string>();
        private List<string> energyLevels = new List<string>();
        private int artistSeperation;

        [Inject]
        private ISongScheduleRepository? SongScheduleRepository { get; set; }

        [Inject]
        private AppStateService? AppStateService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await LoadTables();
            await LoadSongScheduleSettings();
        }

        private async Task LoadTables()
        {
            categories = (await SongScheduleRepository.GetAllCategoriesAsync()).Select(c => c.Name).ToList();
            soundCodes = (await SongScheduleRepository.GetAllSoundCodesAsync()).Select(sc => sc.Code).ToList();
            energyLevels = (await SongScheduleRepository.GetAllEnergyLevelsAsync()).Select(el => el.Level).ToList();
        }
        private async Task LoadSongScheduleSettings()
        {
            artistSeperation = (await SongScheduleRepository.GetSongScheduleSettingsAsync()).ArtistSeperation;
        }

        private async Task AddCategory()
        {
            if (!string.IsNullOrWhiteSpace(newCategory) && AppStateService.station?.StationId != null)
            {
                categories.Add(newCategory);

                var category = new Category
                {
                    Name = newCategory,
                    StationId = (Guid)AppStateService.station?.StationId
                };

                await SongScheduleRepository.AddCategoryAsync(category);

                newCategory = string.Empty;
            }
        }

        private async Task RemoveCategoryHandler(string categoryName)
        {
            var category = (await SongScheduleRepository.GetAllCategoriesAsync()).FirstOrDefault(c => c.Name == categoryName);
            if (category != null)
            {
                categories.Remove(categoryName);
                await SongScheduleRepository.RemoveCategoryAsync(category);
            }
        }

        private async Task AddSoundCode()
        {
            if (!string.IsNullOrWhiteSpace(newSoundCode) && AppStateService.station?.StationId != null)
            {
                soundCodes.Add(newSoundCode);

                var soundCode = new SoundCode
                {
                    Code = newSoundCode,
                    StationId = (Guid)AppStateService.station?.StationId
                };

                await SongScheduleRepository.AddSoundCodeAsync(soundCode);


                newSoundCode = string.Empty;
            }
        }

        private async Task RemoveSoundCodeHandler(string soundcodeName)
        {
            var soundcode = (await SongScheduleRepository.GetAllCategoriesAsync()).FirstOrDefault(c => c.Name == soundcodeName);
            if (soundcode != null)
            {
                categories.Remove(soundcodeName);
                await SongScheduleRepository.RemoveCategoryAsync(soundcode);
            }
        }

        private void AddEnergyLevel()
        {
            if (!string.IsNullOrWhiteSpace(newEnergyLevel))
            {
                energyLevels.Add(newEnergyLevel);
                newEnergyLevel = string.Empty;
            }
        }

        private void RemoveEnergyLevelHandler(string energyLevel)
        {
            energyLevels.Remove(energyLevel);
        }

        private void UpdateSongScheduleSettings(int newArtistSeperation)
        {
            //artistSeperation = newArtistSeperation;
            //var settings = SongScheduleRepository.GetSongScheduleSettingsAsync();
            //settings.ArtistSeperation = newArtistSeperation;
            //await SongScheduleRepository.UpdateSongScheduleSettingsAsync(settings);
        }

    }
}
