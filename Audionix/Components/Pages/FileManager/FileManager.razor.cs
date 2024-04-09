using Audionix.Models;
using Audionix.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.CodeAnalysis;
using Serilog;
using WavesurferBlazorWrapper;


namespace Audionix.Components.Pages.FileManager
{
    partial class FileManager
    {
        private string selectedStation = string.Empty;
        private int progress = 0;
        private WavesurferPlayer? wavePlayer;
        readonly IList<IBrowserFile> filesToUpload = new List<IBrowserFile>();
        IList<AudioFile> filesInDirectory = new List<AudioFile>();
        [Inject]
        public AppSettings? AppSettings { get; set; }
 
        public AudioMetadata audioMetadata { get; set; } = new AudioMetadata();
        private IEnumerable<WavesurferOption> options = new List<WavesurferOption>
            {
                new WavesurferOption(WavesurferOptionKey.hideScrollbar, true),
                new WavesurferOption(WavesurferOptionKey.mediaControls, true)
            };

        public class AudioFile
        {
            public string Name { get; set; }
            public long Size { get; set; }
            public string FullName { get; set; }

            public AudioFile(string name, long size, string fullName)
            {
                Name = name;
                Size = size;
                FullName = fullName;
            }
        }

        private async Task UploadFiles(IReadOnlyList<IBrowserFile> selectedFiles)
        {
            Log.Information("--- FileManager - UploadFiles() -- UploadFiles: {Count}", selectedFiles.Count);
            filesToUpload.Clear();
            foreach (var file in selectedFiles)
            {
                filesToUpload.Add(file);
            }
            await LoadFiles(new List<IBrowserFile>(filesToUpload)); // Clone the list
            GetFolderFileList();
            StateHasChanged();
        }

        private List<IBrowserFile> loadedFiles = new();
        private long maxFileSize = 1024 * 1024 * 1024;

        private async Task LoadFiles(IList<IBrowserFile> selectedFiles)
        {
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
                        var stream = file.OpenReadStream(maxFileSize);

                        var buffer = new byte[81920]; // 80 KB chunks
                        int bytesRead;
                        long totalRead = 0;

                        while ((bytesRead = await stream.ReadAsync(buffer)) != 0)
                        {
                            await fs.WriteAsync(buffer.AsMemory(0, bytesRead));
                            totalRead += bytesRead;

                            // Update progress
                            progress = (int)(totalRead * 100 / file.Size);
                            StateHasChanged(); // Notify Blazor about the state change
                        }

                        await fs.FlushAsync();
                    }

                    loadedFiles.Add(file);
                    await Task.Delay(1000);
                    progress = 0;
                    Log.Information("--- FileManager - LoadFiles() -- File: {Filename} Size: {Size} bytes", file.Name, file.Size);

                    // Get the metadata of the current file

                    var audioMetadata = new AudioMetadataList().GetMetadata(path);

                    Console.WriteLine("=================================");
                    Console.WriteLine("Title: " + audioMetadata.Title);
                    Console.WriteLine("Artist: " + audioMetadata.Artist);
                    Console.WriteLine("Duration: " + audioMetadata.Duration);
                    Console.WriteLine("Intro: " + audioMetadata.Intro);
                    Console.WriteLine("Segue: " + audioMetadata.Segue);
                    Console.WriteLine("=================================");

                    // Create a new AudioMetadata instance and set its properties
                    var audioMetadataForDb = new AudioMetadata
                    {
                        Filename = file.Name,
                        Title = audioMetadata.Title,
                        Artist = audioMetadata.Artist,
                        Duration = audioMetadata.Duration,
                        Intro = audioMetadata.Intro,
                        Segue = audioMetadata.Segue,
                    };

                    // Find the station with the selected call letters and assign its ID to StationId
                    var station = DbContext.Stations.FirstOrDefault(s => s.CallLetters == selectedStation);
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
            }

            await DbContext.SaveChangesAsync();
            GetFolderFileList();
            Log.Information("--- FileManager - LoadFiles() -- End - LoadFiles: {Count}", selectedFiles.Count);

        }
        private string[]? filePaths;
        private void GetFolderFileList()
        {
            Log.Information("--- FileManager - LoadFiles() -- Start - GetFolderFileList: {Station}", selectedStation);
            Log.Debug("--- FileManager - LoadFiles() -- AppSettings.DataPath: {DataPath}", AppSettings?.DataPath);
            if (AppSettings != null && AppSettings.DataPath != null)
            {
                var directoryPath = Path.Combine(AppSettings.DataPath, "Stations", selectedStation.ToString(), "Audio");
                filePaths = Directory.GetFiles(directoryPath);
                filesInDirectory.Clear();

                foreach (var filePath in filePaths)
                {
                    var fileInfo = new FileInfo(filePath);
                    filesInDirectory.Add(new AudioFile(fileInfo.Name, fileInfo.Length, filePath));
                }
            }
            else
            {
                Log.Error("--- FileManager - GetFolderFileList() -- AppSettings is null");
            }
            Log.Information("--- FileManager - GetFolderFileList() -- End - GetFolderFileList: {Station}", selectedStation);
        }

        private void DeleteAudio(AudioFile audioFile)
        {
            Log.Information("--- FileManager - GetFolderFileList() -- DeleteAudio: " + audioFile.Name);
            File.Delete(Path.Combine(AppSettings?.DataPath ?? string.Empty, selectedStation.ToString(), "Audio", audioFile.FullName));
            GetFolderFileList();
            StateHasChanged();
            Log.Information("--- FileManager - GetFolderFileList() - End - DeleteAudio: " + audioFile.Name);
        }

        private async Task EditAudio(AudioFile audioFile)
        {
            Log.Information("--- FileManager - EditAudio() -- EditAudio() START** -aidofile: " + audioFile.Name);

            if (wavePlayer != null)
            {
                await wavePlayer.Stop();
                await wavePlayer.Empty();
                await wavePlayer.RegionClearRegions();

                StateHasChanged();

                //-----------  Begin Requesting File from API -------------
                var request = HttpContextAccessor.HttpContext?.Request;
                if (request != null)
                {
                    var host = request.Host.ToUriComponent();
                    var scheme = request.Scheme;

                    string encodedFilename = System.Net.WebUtility.UrlEncode(audioFile.Name);
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

                            audioMetadata = new AudioMetadataList().GetMetadata(audioFile.FullName);

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
                                                End = (float)audioMetadata.IntroSeconds,
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
                                                Start = (float)(duration.HasValue ? ((audioMetadata.Duration - audioMetadata.Segue) / 1000) : 0),
                                                End = duration.HasValue ? (float)(audioMetadata.Duration / 1000) : 0,
                                                Resize = true,
                                                Color = "rgba(200,10,25,0.3)",
                                                Drag = true,
                                                Id = "Segue"
                                            });
                                    }
                                    else
                                    {
                                        segueRegion.Start = (float)(duration.HasValue ? ((audioMetadata.Duration - audioMetadata.Segue) / 1000) : 0);
                                        segueRegion.End = duration.HasValue ? (float)(audioMetadata.Duration / 1000) : 0;
                                    }
                                    await wavePlayer.RegionListUpdate(regions);
                                }
                            }
                            else
                            {
                                Log.Error("++++++ FileManager - EditAudio() -- No wavePlayer found");
                            }
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

    }
}
