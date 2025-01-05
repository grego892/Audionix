using Audionix.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Serilog;
using WavesurferBlazorWrapper;
using MudBlazor;
using SharedLibrary.Models;
using SharedLibrary.Models.MusicSchedule;
using SharedLibrary.Repositories;

namespace Audionix.Components.Pages.MusicSchedule
{
    partial class SongStructure : IDisposable
    {
        private string selectedFolder = string.Empty;
        private string selectedSongCategory = string.Empty;
        IList<AudioMetadata> filesInDirectory = new List<AudioMetadata>();
        private List<string>? folders;
        private List<SongCategory>? songCategories;
        //private List<EventType>? EventTypes;
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

        protected override async Task OnInitializedAsync()
        {
            Log.Information("--- FileManager - OnInitializedAsync() -- Initializing FileManager Page");
            if (AppStateService?.station != null && FileManagerService != null)
            {
                folders = await FileManagerService.GetFoldersForStation(AppStateService.station.StationId.ToString());
                songCategories = await SongCategoryRepository.GetSongCategoriesAsync(AppStateService.station.StationId);
                //EventTypes = Enum.GetValues(typeof(EventType)).Cast<EventType>().ToList();
                AppStateService.OnStationChanged += HandleStationChanged;
            }
        }

        private async void HandleStationChanged(object? sender, EventArgs e)
        {
            if (AppStateService?.station != null && FileManagerService != null)
            {
                folders = await FileManagerService.GetFoldersForStation(AppStateService.station.StationId.ToString());
                songCategories = await SongCategoryRepository.GetSongCategoriesAsync(AppStateService.station.StationId);

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
