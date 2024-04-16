using Audionix.Models;
using Audionix.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Serilog;
using WavesurferBlazorWrapper;

namespace Audionix.Components.Pages.FileManager
{
    partial class FileManager
    {
        private string selectedStation = string.Empty;
        private bool isUploading;
        private int progress;
        private WavesurferPlayer? wavePlayer;
        readonly List<IBrowserFile> filesToUpload = new List<IBrowserFile>();
        IList<AudioMetadata> filesInDirectory = new List<AudioMetadata>();
        private List<Station>? stations;
        public AudioMetadata? audioMetadata { get; set; } = new AudioMetadata();

        [Inject] public AppSettings? AppSettings { get; set; }
        [Inject] public IConfiguration? Configuration { get; set; }
        [Inject] private IHttpContextAccessor? HttpContextAccessor { get; set; }
        [Inject] public FileManagerService? FileManagerSvc { get; set; }
        [Inject] public StationService? StationSvc { get; set; }
        [Inject] public AudionixDbContext DbContext { get; set; }

        public string EditorTitle = string.Empty;
        public string EditorArtist = string.Empty;
        public double EditorIntro { get; set; } = 0;
        public double EditorSegue { get; set; } = 0;
        public double EditorDuration { get; set; } = 0;

        protected override async Task OnInitializedAsync()
        {
            stations = await DbContext.Stations.AsNoTracking().ToListAsync();
            filesInDirectory = await DbContext.AudioMetadatas.AsNoTracking().ToListAsync();
        }
        private void updateProgress(int progress)
        {
            this.progress = progress;
        }
        private async Task UploadFiles(IReadOnlyList<IBrowserFile> selectedFiles)
        {
            isUploading = true;
            progress = 0;
            await FileManagerSvc?.UploadFiles(selectedFiles, selectedStation, filesToUpload, filesInDirectory, () => LoadFiles(selectedFiles, selectedStation, updateProgress), GetFolderFileList, updateProgress);
            isUploading = false;
            progress = 0;
        }


        public async Task LoadFiles(IReadOnlyList<IBrowserFile> selectedFiles, string selectedStation, Action<int> updateProgress)
        {
            isUploading = true;
            progress = 0;
            await FileManagerSvc?.LoadFiles(selectedFiles as IReadOnlyList<IBrowserFile>, selectedStation, updateProgress);
            GetFolderFileList();
        }


        private void GetFolderFileList()
        {
            filesInDirectory = StationSvc?.GetFolderFileList(selectedStation, stations, DbContext) ?? new List<AudioMetadata>();
        }

        private async Task DeleteAudioAsync(AudioMetadata audioMetadata)
        {
            await FileManagerSvc?.DeleteAudioAsync(audioMetadata, selectedStation, AppSettings?.DataPath ?? string.Empty, DbContext, GetFolderFileList);
        }

        public string SelectedStation
        {
            get => selectedStation;
            set
            {
                if (selectedStation != value)
                {
                    selectedStation = value;
                    OnSelectedValueChanged(value);
                }
            }
        }

        private void OnSelectedValueChanged(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                Log.Information("--- FileManager - OnSelectedValuesChanged() -- SelectedStation: {Station}", value);
                SelectedStation = value;
                GetFolderFileList();
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
    }
}
