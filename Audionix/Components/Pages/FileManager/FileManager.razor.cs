using Audionix.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Serilog;
using WavesurferBlazorWrapper;
using MudBlazor;
using SharedLibrary.Models;
using SharedLibrary.Models.MusicSchedule;
using Audionix.Repositories;

namespace Audionix.Components.Pages.FileManager
{
    partial class FileManager : IDisposable
    {
        private string selectedFolder = string.Empty;
        private string selectedCategory = string.Empty;

        private bool isUploading;
        private int progress;
        private WavesurferPlayer? wavePlayer;
        readonly List<IBrowserFile> filesToUpload = new List<IBrowserFile>();
        IList<AudioMetadata> filesInDirectory = new List<AudioMetadata>();
        private List<string>? folders;
        private List<Category>? categories;
        private List<AudioType>? audioTypes;
        private bool isSongdataEnabled = false;
        private bool editorEnabled = false;
        public AudioMetadata? audioMetadata { get; set; } = new AudioMetadata();

        [Inject] public IAppSettingsRepository? AppSettingsRepository { get; set; }
        [Inject] private IHttpContextAccessor? HttpContextAccessor { get; set; }
        [Inject] public FileManagerService? FileManagerSvc { get; set; }
        [Inject] private IStationRepository StationRepository { get; set; } = default!;
        [Inject] private ICategoryRepository CategoryRepository { get; set; } = default!;
        [Inject] private IAudioMetadataRepository AudioMetadataRepository { get; set; } = default!;
        [Inject] public AppStateService? AppStateService { get; set; }
        [Inject] FileManagerService? FileManagerService { get; set; }
        [Inject] ISnackbar? Snackbar { get; set; }

        public string EditorTitle = string.Empty;
        public string EditorArtist = string.Empty;
        public double EditorIntro { get; set; } = 0;
        public double EditorSegue { get; set; } = 0;
        public double EditorDuration { get; set; } = 0;

        protected override async Task OnInitializedAsync()
        {
            Log.Information("--- FileManager - OnInitializedAsync() -- Initializing FileManager Page");
            if (AppStateService?.station != null)
            {
                folders = await FileManagerService.GetFoldersForStation(AppStateService.station.StationId.ToString());
                categories = await CategoryRepository.GetCategoriesAsync(AppStateService.station.StationId);
                audioTypes = Enum.GetValues(typeof(AudioType)).Cast<AudioType>().ToList();
                AppStateService.OnStationChanged += HandleStationChanged;
            }
        }

        private async void HandleStationChanged(object? sender, EventArgs e)
        {
            if (AppStateService?.station != null)
            {
                folders = await FileManagerService.GetFoldersForStation(AppStateService.station.StationId.ToString());
                categories = await CategoryRepository.GetCategoriesAsync(AppStateService.station.StationId);

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
            }
        }

        private async Task UploadFiles(IReadOnlyList<IBrowserFile> selectedFiles)
        {
            isUploading = true;
            progress = 0;
            if (FileManagerService != null && AppStateService?.station != null)
            {
                var duplicateFiles = await FileManagerService.UploadFiles(selectedFiles, AppStateService.station.StationId, selectedFolder, filesToUpload, filesInDirectory, updateProgress);

                if (Snackbar != null)
                {
                    foreach (var file in duplicateFiles)
                    {
                        Snackbar.Add("Folder with the same name already exists for this station", Severity.Error);
                    }
                }
            }
            isUploading = false;
            progress = 0;
            await GetFolderFileList(SelectedFolder);
        }

        private async Task SongCategoryChanged(AudioMetadata audioMetadata, string newCategory)
        {
            if (AudioMetadataRepository != null)
            {
                audioMetadata.Category = newCategory;
                await AudioMetadataRepository.UpdateAudioMetadataAsync(audioMetadata);
                Snackbar?.Add("Category updated successfully", Severity.Success);
            }
        }

        private async Task SongAudioTypeChanged(AudioMetadata audioMetadata, AudioType audioType)
        {
            if (AudioMetadataRepository != null)
            {
                audioMetadata.AudioType = audioType;
                await AudioMetadataRepository.UpdateAudioMetadataAsync(audioMetadata);
                Snackbar?.Add("Audio Type updated successfully", Severity.Success);
            }
        }

        private async Task DeleteAudioAsync(AudioMetadata audioMetadata)
        {
            if (FileManagerSvc != null && AppStateService?.station != null)
            {
                var appSettings = await AppSettingsRepository?.GetAppSettingsAsync();
                var dataPath = appSettings?.DataPath ?? string.Empty;
                await FileManagerSvc.DeleteAudioAsync(audioMetadata, AppStateService.station.CallLetters, dataPath, async () => await GetFolderFileList(SelectedFolder));
            }
        }

        public double RoundedEditorIntro
        {
            get
            {
                return Math.Round(EditorIntro / 1000.0, 1);
            }
            set
            {
                EditorIntro = value * 1000;
            }
        }
        public double RoundedEditorSegue
        {
            get
            {
                return Math.Round(EditorSegue / 1000.0, 1);
            }
            set
            {
                EditorSegue = value * 1000;
            }
        }

        private void updateProgress(int value)
        {
            progress = value;
            StateHasChanged();
        }

        private void SongdataPressed()
        {
            isSongdataEnabled = !isSongdataEnabled;
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

        public void Dispose()
        {
            if (AppStateService != null)
            {
                AppStateService.OnStationChanged -= HandleStationChanged;
            }
        }
    }
}
