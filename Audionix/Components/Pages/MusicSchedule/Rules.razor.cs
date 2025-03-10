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
        private List<Category> categories = new();
        private List<Category> filteredCategories = new();
        private List<string> soundCodes = new List<string>();
        private List<string> energyLevels = new List<string>();
        private int artistSeperation;

        [Inject]
        private ISongScheduleRepository? SongScheduleRepository { get; set; }

        [Inject]
        private AppStateService? AppStateService { get; set; }

        protected override async Task OnInitializedAsync()
        {

            AppStateService.OnStationChanged += HandleStationChanged;



            await LoadTables();
            await LoadSongScheduleSettings();
        }

        private async void HandleStationChanged(object? sender, EventArgs e)
        {
            StateHasChanged();
        }

        private async Task LoadTables()
        {
            categories = (await SongScheduleRepository.GetCategoriesAsync(AppStateService.station.StationId));
            soundCodes = (await SongScheduleRepository.GetAllSoundCodesAsync()).Select(sc => sc.Code).ToList();
            energyLevels = (await SongScheduleRepository.GetAllEnergyLevelsAsync()).Select(el => el.Level).ToList();
        }
        private async Task LoadSongScheduleSettings()
        {
            artistSeperation = (await SongScheduleRepository.GetSongScheduleSettingsAsync()).ArtistSeperation;
        }

        //private async Task AddCategory()
        //{
        //    if (!string.IsNullOrWhiteSpace(newCategory) && AppStateService.station?.StationId != null)
        //    {
        //        categories.Add(newCategory);

        //        var category = new Category
        //        {
        //            Name = newCategory,
        //            StationId = (Guid)AppStateService.station?.StationId
        //        };

        //        await SongScheduleRepository.AddCategoryAsync(category);

        //        newCategory = string.Empty;
        //    }
        //}

        private async Task AddCategory()
        {
            if (AppStateService.station != null && !string.IsNullOrEmpty(newCategory))
            {
                var category = new Category
                {
                    CategoryName = newCategory,
                    StationId = AppStateService.station.StationId,
                    CategoryId = Guid.NewGuid()
                };
                await SongScheduleRepository.AddCategoryAsync(category);
                categories.Add(category);
                newCategory = string.Empty;
            }
        }

        //private async Task RemoveCategoryHandler(string categoryName)
        //{
        //    var category = (await SongScheduleRepository.GetCategoriesAsync(AppStateService.station.StationId));
        //    if (category != null)
        //    {
        //        categories.Remove(category);
        //        await SongScheduleRepository.RemoveCategoryAsync(category);
        //    }
        //}
        private async Task RemoveCategoryHandler(string categoryName)
        {
            if (AppStateService?.station != null)
            {
                var category = categories.FirstOrDefault(c => c.CategoryName == categoryName);
                if (category != null)
                {
                    categories.Remove(category);
                    await SongScheduleRepository.RemoveCategoryAsync(category);
                }
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
            var soundcode = (await SongScheduleRepository.GetAllSoundCodesAsync()).FirstOrDefault(sc => sc.Code == soundcodeName);
            if (soundcode != null)
            {
                soundCodes.Remove(soundcodeName);
                await SongScheduleRepository.RemoveSoundCodeAsync(soundcode);
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

        private async Task UpdateSongScheduleSettings(int newArtistSeperation)
        {
            artistSeperation = newArtistSeperation;
            var settings = await SongScheduleRepository.GetSongScheduleSettingsAsync();
            settings.ArtistSeperation = newArtistSeperation;
            await SongScheduleRepository.UpdateSongScheduleSettingsAsync(settings);
        }
    }
}
