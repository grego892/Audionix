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

        private async Task UploadFiles(IReadOnlyList<IBrowserFile> selectedFiles)
        {
            await FileManagerSvc?.UploadFiles(selectedFiles, filesToUpload, filesInDirectory, () => LoadFiles(new List<IBrowserFile>(filesToUpload)), GetFolderFileList);
        }

        private async Task LoadFiles(IList<IBrowserFile> selectedFiles)
        {
            isUploading = true;
            progress = 0;
            await FileManagerSvc?.LoadFiles(selectedFiles, selectedStation);
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


        private async Task EditAudio(AudioMetadata audioMetadata)
        {
            isUploading = true;
            progress = 0;
            Log.Information("--- FileManager - EditAudio() -- EditAudio() START** -aidofile: " + audioMetadata.Filename);

            if (wavePlayer != null)
            {
                await wavePlayer.Stop();
                await wavePlayer.Empty();
                await wavePlayer.RegionClearRegions();

                //-----------  Begin Requesting File from API -------------
                var request = HttpContextAccessor?.HttpContext?.Request;
                if (request != null)
                {
                    var host = request.Host.ToUriComponent();
                    var scheme = request.Scheme;

                    string encodedFilename = System.Net.WebUtility.UrlEncode(audioMetadata.Filename);
                    string url = $"{scheme}://{host}/api/audio/{selectedStation}/{encodedFilename}";
                    Log.Information("--- FileManager - EditAudio() -- EditAudio sending to API: " + url);

                    try
                    {
                        var httpClient = new HttpClient();
                        var response = await httpClient.GetAsync(url);
                        Log.Information("--- FileManager - EditAudio() -- EditAudio response: " + response.StatusCode);

                        if (response.IsSuccessStatusCode)
                        {
                            var contentStream = await response.Content.ReadAsStreamAsync();
                            wavePlayer?.Load(url);

                            audioMetadata = DbContext.AudioMetadatas.FirstOrDefault(am => am.Filename == audioMetadata.Filename);

                            if (wavePlayer != null)
                            {
                                IEnumerable<WavesurferRegion>? regions = await wavePlayer.RegionList();

                                if (regions != null)
                                {
                                    var introRegion = regions.FirstOrDefault(r => r.Id == "Intro");

                                    if (introRegion == null)
                                    {
                                        await wavePlayer.RegionAddRegion(
                                            new WavesurferRegion()
                                            {
                                                Start = 0,
                                                End = ((float)audioMetadata.Intro) / 1000,
                                                Resize = true,
                                                Color = "rgba(10,200,25,0.3)",
                                                Drag = true,
                                                Id = "Intro"
                                            });
                                    }
                                    else
                                    {
                                        introRegion.Start = 0;
                                        introRegion.End = (float)audioMetadata.IntroSeconds;
                                    }
                                    await wavePlayer.RegionListUpdate(regions);

                                    float? duration = await wavePlayer.GetDuration();

                                    var segueRegion = regions.FirstOrDefault(r => r.Id == "Segue");
                                    if (segueRegion == null)
                                    {
                                        await wavePlayer.RegionAddRegion(
                                            new WavesurferRegion()
                                            {
                                                Start = (float)audioMetadata.Duration - ((audioMetadata.Segue) / 1000),
                                                End = duration.HasValue ? (float)audioMetadata.Duration : 0,
                                                Resize = true,
                                                Color = "rgba(200,10,25,0.3)",
                                                Drag = true,
                                                Id = "Segue"
                                            });
                                    }
                                    else
                                    {
                                        segueRegion.Start = (float)(duration.HasValue ? ((audioMetadata.Duration - audioMetadata.Segue)) : 0);
                                        segueRegion.End = duration.HasValue ? (float)(audioMetadata.Duration) : 0;
                                    }
                                    await wavePlayer.RegionListUpdate(regions);
                                }
                            }
                            else
                            {
                                Log.Error("++++++ FileManager - EditAudio() -- No wavePlayer found");
                            }

                            EditorTitle = audioMetadata?.Title ?? string.Empty;
                            EditorArtist = audioMetadata?.Artist ?? string.Empty;
                            EditorIntro = audioMetadata?.Intro ?? 0;
                            EditorSegue = audioMetadata?.Segue ?? 0;
                            StateHasChanged();

                        }
                        else
                        {
                            Log.Error("++++++ FileManager - EditAudio() -- Error making HTTP request");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "++++++ FileManager - EditAudio() - FileManager - EditAudio() - Error making HTTP request");
                    }
                }
                else
                {
                    Log.Error("--- FileManager - EditAudio() -- No HttpContextAccessor.HttpContext?.Request found");
                }
            }
            else
            {
                Log.Error("++++++ FileManager - EditAudio() -- No wavePlayer found");
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
