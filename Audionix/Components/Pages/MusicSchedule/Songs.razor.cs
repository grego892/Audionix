using Audionix.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Serilog;
using WavesurferBlazorWrapper;
using MudBlazor;
using SharedLibrary.Models;
using SharedLibrary.Models.MusicSchedule;
using SharedLibrary.Repositories;
using SharedLibrary.Models.MusicSchedule.Rules;

namespace Audionix.Components.Pages.MusicSchedule
{
    partial class Songs : IDisposable
    {
        private string selectedFolder = string.Empty;
        private string selectedSongCategory = string.Empty;
        private string selectedSoundCode = string.Empty;
        private string selectedEnergyLevel = string.Empty;
        IList<AudioMetadata> filesInDirectory = new List<AudioMetadata>();
        private List<string>? folders;
        private List<SongCategory>? categories;
        private List<SoundCode>? soundCodes;
        private List<EnergyLevel>? energyLevels;

        public AudioMetadata? audioMetadata { get; set; } = new AudioMetadata();

        [Inject] public IAppSettingsRepository? AppSettingsRepository { get; set; }
        [Inject] private IHttpContextAccessor? HttpContextAccessor { get; set; }
        [Inject] public FileManagerService? FileManagerSvc { get; set; }
        [Inject] private IStationRepository StationRepository { get; set; } = default!;
        [Inject] private ISongCategoryRepository SongCategoryRepository { get; set; } = default!;
        [Inject] private IAudioMetadataRepository AudioMetadataRepository { get; set; } = default!;
        [Inject] public AppStateService? AppStateService { get; set; }
        [Inject] FileManagerService? FileManagerService { get; set; }
        [Inject] ISnackbar? Snackbar { get; set; }
        [Inject] private ISongScheduleRepository SongScheduleRepository { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            Log.Information("--- FileManager - OnInitializedAsync() -- Initializing FileManager Page");
            if (AppStateService?.station != null && FileManagerService != null)
            {
                folders = await FileManagerService.GetFoldersForStation(AppStateService.station.StationId.ToString());
                categories = await SongScheduleRepository.GetAllCategoriesAsync();
                soundCodes = await SongScheduleRepository.GetAllSoundCodesAsync();
                energyLevels = await SongScheduleRepository.GetAllEnergyLevelsAsync();
                AppStateService.OnStationChanged += HandleStationChanged;
            }
        }

        private async void HandleStationChanged(object? sender, EventArgs e)
        {
            if (AppStateService?.station != null && FileManagerService != null)
            {
                folders = await FileManagerService.GetFoldersForStation(AppStateService.station.StationId.ToString());
                filesInDirectory.Clear();
                SelectedFolder = string.Empty;

                StateHasChanged();
            }
        }

        private async Task GetFolderFileList(string selectedFolder)
        {
            SelectedFolder = selectedFolder;

            if (FileManagerService != null && AppStateService?.station != null)
            {
                filesInDirectory = await FileManagerService.GetFolderFileList(AppStateService.station.StationId, selectedFolder);

                // Populate the Category, SoundCode, and EnergyLevel information for each AudioMetadata
                foreach (var file in filesInDirectory)
                {
                    if (file.CategoryId.HasValue)
                    {
                        file.SongCategory = categories?.FirstOrDefault(c => c.SongCategoryId == file.CategoryId.Value);
                    }
                    if (file.SoundCodeId.HasValue)
                    {
                        file.SoundCode = soundCodes?.FirstOrDefault(sc => sc.Id == file.SoundCodeId.Value);
                    }
                    if (file.EnergyLevelId.HasValue)
                    {
                        file.EnergyLevel = energyLevels?.FirstOrDefault(el => el.Id == file.EnergyLevelId.Value);
                    }
                }
            }
        }


        public string SelectedFolder
        {
            get => selectedFolder;
            set
            {
                if (selectedFolder != value)
                {
                    selectedFolder = value;
                    filesInDirectory.Clear();
                    StateHasChanged();
                }
            }
        }

        private async Task OnCategoryChanged(string newCategory, AudioMetadata audioMetadata)
        {
            if (audioMetadata != null)
            {
                var category = categories?.FirstOrDefault(c => c.SongCategoryName == newCategory);
                if (category != null)
                {
                    audioMetadata.SongCategory = category;
                    await AudioMetadataRepository.UpdateAudioMetadataAsync(audioMetadata);
                }
            }
        }

        private async Task OnSoundCodeChanged(string newSoundCode, AudioMetadata audioMetadata)
        {
            if (audioMetadata != null)
            {
                var soundCode = soundCodes?.FirstOrDefault(c => c.Code == newSoundCode);
                if (soundCode != null)
                {
                    audioMetadata.SoundCode = soundCode;
                    audioMetadata.SoundCodeId = soundCode.Id;
                    await AudioMetadataRepository.UpdateAudioMetadataAsync(audioMetadata);
                }
            }
        }

        private async Task OnEnergyLevelChanged(string newEnergyLevel, AudioMetadata audioMetadata)
        {
            if (audioMetadata != null)
            {
                var energyLevel = energyLevels?.FirstOrDefault(c => c.Level == newEnergyLevel);
                if (energyLevel != null)
                {
                    audioMetadata.EnergyLevel = energyLevel; // Update the EnergyLevel property
                    audioMetadata.EnergyLevelId = energyLevel.Id;
                    await AudioMetadataRepository.UpdateAudioMetadataAsync(audioMetadata);
                }
            }
        }

        public void Dispose()
        {
            if (AppStateService != null)
            {
                AppStateService.OnStationChanged -= HandleStationChanged;
            }
        }
    }
}

