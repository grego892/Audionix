using Audionix.Models;
using Audionix.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Serilog;
using WavesurferBlazorWrapper;
using System.Threading.Tasks;
using MudBlazor;

namespace Audionix.Components.Pages.FileManager
{
    partial class FileManager
    {
        private string selectedStation = string.Empty;
        private string selectedFolder = string.Empty;
        private bool isUploading;
        private int progress;
        private WavesurferPlayer? wavePlayer;
        readonly List<IBrowserFile> filesToUpload = new List<IBrowserFile>();
        IList<AudioMetadata> filesInDirectory = new List<AudioMetadata>();
        private List<Station>? stations;
        private List<string>? folders;
        public AudioMetadata? audioMetadata { get; set; } = new AudioMetadata();

        [Inject] public AppSettings? AppSettings { get; set; }
        [Inject] private IHttpContextAccessor? HttpContextAccessor { get; set; }
        [Inject] public FileManagerService? FileManagerSvc { get; set; }
        [Inject] public StationService? StationSvc { get; set; }
        [Inject] public AudionixDbContext? DbContext { get; set; }
        [Inject] FileManagerService? FileManagerService { get; set; }
        [Inject] ISnackbar? Snackbar { get; set; }


        public string EditorTitle = string.Empty;
        public string EditorArtist = string.Empty;
        public double EditorIntro { get; set; } = 0;
        public double EditorSegue { get; set; } = 0;
        public double EditorDuration { get; set; } = 0;

        protected override async Task OnInitializedAsync()
        {
            if (DbContext != null)
            {
                stations = await DbContext.Stations.AsNoTracking().ToListAsync();
                filesInDirectory = await DbContext.AudioFiles.AsNoTracking().ToListAsync();
            }
            GetFolderFileList();
        }

        private async Task UploadFiles(IReadOnlyList<IBrowserFile> selectedFiles)
        {
            isUploading = true;
            progress = 0;
            var duplicateFiles = await FileManagerService.UploadFiles(selectedFiles, selectedStation, selectedFolder, filesToUpload, filesInDirectory, updateProgress);

            if (Snackbar != null)
            {
                foreach (var file in duplicateFiles)
                {
                    Snackbar.Add("Folder with the same name already exists for this station", Severity.Error);
                }
            }
            isUploading = false;
            progress = 0;
        }


        private void GetFolderFileList()
        {
            if (StationSvc != null && DbContext != null)
            {
                filesInDirectory = StationSvc.GetFolderFileList(selectedStation, stations, DbContext) ?? new List<AudioMetadata>();
            }
        }

        private async Task DeleteAudioAsync(AudioMetadata audioMetadata)
        {
            if (FileManagerSvc != null && DbContext != null)
            {
                await FileManagerSvc.DeleteAudioAsync(audioMetadata, selectedStation, AppSettings?.DataPath ?? string.Empty, DbContext, GetFolderFileList);
            }
        }

        public string SelectedStation
        {
            get => selectedStation;
            set
            {
                if (selectedStation != value)
                {
                    selectedStation = value;
                    OnSelectedStationValueChanged(value);
                }
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

        private async void OnSelectedStationValueChanged(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                Log.Information($"--- FileManager -  OnSelectedStationValueChanged() -- SelectedStation: {value}", value);
                filesInDirectory.Clear();
                selectedFolder = string.Empty;
                SelectedStation = value;
                var station = stations?.FirstOrDefault(s => s.CallLetters == value);
                if (station != null && FileManagerSvc != null)
                {
                    folders = await FileManagerSvc.GetFoldersForStation(station.Id.ToString());
                }
            }
        }

        private void OnSelectedStationValueChangedWrapper(string value)
        {
            
            folders = new List<string>();
            StateHasChanged();
            OnSelectedStationValueChanged(value);
        }

    }
}
