using Audionix.Models;
using Audionix.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using Serilog;
using System.Configuration;
using System.Linq;
using WavesurferBlazorWrapper;


namespace Audionix.Components.Pages.FileManager
{
    partial class FileManager
    {
        private string selectedStation = string.Empty;
        private int progress = 0;
        private bool isUploading;
        private WavesurferPlayer? wavePlayer;
        readonly List<IBrowserFile> filesToUpload = new List<IBrowserFile>();
        IList<AudioMetadata> filesInDirectory = new List<AudioMetadata>();
        private List<Station>? stations;
        public AudioMetadata? audioMetadata { get; set; } = new AudioMetadata();

        [Inject] public AppSettings? AppSettings { get; set; }
        [Inject] public IConfiguration? Configuration { get; set; }
        [Inject] private IHttpContextAccessor? HttpContextAccessor { get; set; }
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
            Log.Information("--- FileManager - UploadFiles() -- UploadFiles: {Count}", selectedFiles.Count);

            // Filter out the files that already exist before adding them to filesToUpload
            var distinctFiles = selectedFiles.Where(file => !filesInDirectory.Any(f => f.Filename == file.Name)).ToList();
            filesToUpload.Clear();
            filesToUpload.AddRange(distinctFiles);

            await LoadFiles(new List<IBrowserFile>(filesToUpload)); // Clone the list
            GetFolderFileList();
        }


        private List<IBrowserFile> loadedFiles = new();
        private long maxFileSize = 1024 * 1024 * 1024;

        private async Task LoadFiles(IList<IBrowserFile> selectedFiles)
        {
            isUploading = true;
            Log.Information("--- FileManager - LoadFiles() -- LoadFiles: {Count}", selectedFiles.Count);
            loadedFiles.Clear();

            foreach (var file in selectedFiles)
            {
                try
                {
                    var path = Path.Combine(AppSettings?.DataPath ?? string.Empty, "Stations", selectedStation.ToString(), "Audio", file.Name);

                    // Use a using statement to ensure the FileStream is disposed of
                    await using (var fs = new FileStream(path, FileMode.Create))
                    {
                        isUploading = true;
                        var stream = file.OpenReadStream(maxFileSize);

                        var buffer = new byte[81920]; // 80 KB chunks
                        int bytesRead;
                        long totalRead = 0;

                        var lastUpdate = DateTime.Now;
                        while ((bytesRead = await stream.ReadAsync(buffer)) != 0)
                        {
                            await fs.WriteAsync(buffer.AsMemory(0, bytesRead));
                            totalRead += bytesRead;

                            // Update progress
                            if ((DateTime.Now - lastUpdate).TotalSeconds >= .25)
                            {
                                progress = (int)(totalRead * 100 / file.Size);
                                StateHasChanged();
                                lastUpdate = DateTime.Now;
                            }
                        }

                        await fs.FlushAsync();
                    }

                    loadedFiles.Add(file);
                    await Task.Delay(1000);
                    isUploading = false;
                    progress = 0;
                    Log.Information("--- FileManager - LoadFiles() -- File: {Filename} Size: {Size} bytes", file.Name, file.Size);

                    var audioMetadata = new AudioMetadataList().GetMetadata(path);

                    // Create a new AudioMetadata instance and set its properties
                    var audioMetadataForDb = new AudioMetadata
                    {
                        Filename = file.Name,
                        Title = audioMetadata.Title,
                        Artist = audioMetadata.Artist,
                        Duration = audioMetadata.Duration,
                        Intro = audioMetadata.Intro,
                        Segue = audioMetadata.Segue
                    };

                    // Find the station with the selected call letters and assign its ID to StationId
                    var station = DbContext.Stations.AsNoTracking().FirstOrDefault(s => s.CallLetters == selectedStation);
                    if (station != null)
                    {
                        audioMetadataForDb.StationId = station.Id;
                    }
                    else
                    {
                        Log.Error("Station with call letters {CallLetters} not found", selectedStation);
                    }

                    // Add the new audio metadata to the database
                    DbContext.AudioMetadatas.Add(audioMetadataForDb);
                }
                catch (Exception ex)
                {
                    Log.Error("++++++ FileManager - LoadFiles() -- File: {Filename} Error: {Error}",
                        file.Name, ex.Message);
                }

                isUploading = false;
            }

            await DbContext.SaveChangesAsync();
            GetFolderFileList();
            Log.Information("--- FileManager - LoadFiles() -- End - LoadFiles: {Count}", selectedFiles.Count);
        }


        private void GetFolderFileList()
        {
            Log.Information("--- FileManager - GetFolderFileList() -- Start");
            filesInDirectory.Clear();

            if (!string.IsNullOrEmpty(selectedStation) && stations != null)
            {
                var station = stations.FirstOrDefault(s => s.CallLetters == selectedStation);
                if (station != null)
                {
                    filesInDirectory = DbContext.AudioMetadatas
                        .AsNoTracking()
                        .Where(am => am.StationId == station.Id)
                        .ToList();
                }
            }

            Log.Information("--- FileManager - GetFolderFileList() -- End - filesInDirectory: {Count}", filesInDirectory.Count);
        }

        private async Task DeleteAudioAsync(AudioMetadata audioMetadata)
        {
            Log.Information("--- FileManager - GetFolderFileList() -- DeleteAudio: " + audioMetadata.Filename);
            File.Delete(Path.Combine(AppSettings?.DataPath ?? string.Empty, "Stations", selectedStation.ToString(), "Audio", audioMetadata.Filename));
            DbContext.AudioMetadatas.Remove(audioMetadata);
            await DbContext.SaveChangesAsync();
            GetFolderFileList();
            Log.Information("--- FileManager - GetFolderFileList() - End - DeleteAudio: " + audioMetadata.Filename);
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
                            EditorIntro = audioMetadata.Intro;
                            EditorSegue = audioMetadata.Segue;
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
