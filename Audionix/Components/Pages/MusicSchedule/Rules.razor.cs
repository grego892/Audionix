using Microsoft.AspNetCore.Components;
using SharedLibrary.Repositories;
using SharedLibrary.Models.MusicSchedule.Rules;
using Audionix.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedLibrary.Models.MusicSchedule;

namespace Audionix.Components.Pages.MusicSchedule
{
    public partial class Rules
    {
        private string? newCategory;
        private string? newSoundCode;
        private string? newEnergyLevel;
        private List<SongCategory> categories = new();
        private List<string> soundCodes = new();
        private List<string> energyLevels = new();
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
            if (AppStateService?.station != null)
            {
                categories = await SongScheduleRepository.GetCategoriesAsync(AppStateService.station.StationId);
                soundCodes = (await SongScheduleRepository.GetSoundCodesAsync(AppStateService.station.StationId)).Select(sc => sc.Code).ToList();
                energyLevels = (await SongScheduleRepository.GetEnergyLevelsAsync(AppStateService.station.StationId)).Select(el => el.Level).ToList();
            }
        }

        private async Task LoadSongScheduleSettings()
        {
            var settings = await SongScheduleRepository.GetSongScheduleSettingsAsync();
            if (settings != null)
            {
                artistSeperation = settings.ArtistSeperation;
            }
        }

        private async Task AddCategory()
        {
            if (AppStateService?.station != null && !string.IsNullOrEmpty(newCategory))
            {
                var category = new SongCategory
                {

                    SongCategoryName = newCategory,
                    StationId = AppStateService.station.StationId
                };
                await SongScheduleRepository.AddCategoryAsync(category);
                categories.Add(category);
                newCategory = string.Empty;
            }
        }

        private async Task RemoveCategoryHandler(string categoryName)
        {
            if (AppStateService?.station != null)
            {
                var category = categories.FirstOrDefault(c => c.SongCategoryName == categoryName);
                if (category != null)
                {
                    categories.Remove(category);
                    await SongScheduleRepository.RemoveCategoryAsync(category);
                }
            }
        }

        private async Task AddSoundCode()
        {
            if (!string.IsNullOrWhiteSpace(newSoundCode) && AppStateService?.station?.StationId != null)
            {
                soundCodes.Add(newSoundCode);

                var soundCode = new SoundCode
                {
                    Code = newSoundCode,
                    StationId = AppStateService.station.StationId
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

        private async Task AddEnergyLevel()
        {
            if (!string.IsNullOrWhiteSpace(newEnergyLevel))
            {
                energyLevels.Add(newEnergyLevel);

                var energryLevel = new EnergyLevel
                {
                    Level = newEnergyLevel,
                    StationId = AppStateService.station.StationId
                };

                await SongScheduleRepository.AddEnergyLevelAsync(energryLevel);

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
            if (settings != null)
            {
                settings.ArtistSeperation = newArtistSeperation;
                await SongScheduleRepository.UpdateSongScheduleSettingsAsync(settings);
            }
        }
    }
}
