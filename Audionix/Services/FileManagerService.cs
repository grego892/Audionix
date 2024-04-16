using Audionix.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Audionix.Services
{
    public class FileManagerService
    {
        private const int BufferSize = 81920; // 80 KB chunks
        long maxFileSize = 2L * 1024 * 1024 * 1024; // 2 GB
        private readonly AudionixDbContext _dbContext;
        private readonly AppSettings? _appSettings;

        public FileManagerService(AudionixDbContext dbContext, AppSettings? appSettings)
        {
            _dbContext = dbContext;
            _appSettings = appSettings;
        }

        public async Task<List<IBrowserFile>> UploadFiles(IReadOnlyList<IBrowserFile> selectedFiles, string selectedStation, List<IBrowserFile> filesToUpload, IList<AudioMetadata> filesInDirectory, Func<Task> loadFiles, Action getFolderFileList, Action<int> updateProgress)
        {
            Log.Information("--- FileManager - UploadFiles() -- UploadFiles: {Count}", selectedFiles.Count);

            // Filter out the files that already exist in the database before adding them to filesToUpload
            var distinctFiles = selectedFiles.Where(file => !_dbContext.AudioMetadatas.Any(f => f.Filename == file.Name)).ToList();
            var existingFiles = selectedFiles.Except(distinctFiles).ToList();

            filesToUpload.Clear();
            filesToUpload.AddRange(distinctFiles);

            await LoadFiles(selectedFiles, selectedStation, updateProgress);
            getFolderFileList();

            return existingFiles;
        }



        public async Task LoadFiles(IReadOnlyList<IBrowserFile> selectedFiles, string selectedStation, Action<int> updateProgress)
        {
            Log.Information("--- FileManager - LoadFiles() -- LoadFiles: {Count}", selectedFiles.Count);

            foreach (var file in selectedFiles)
            {
                var path = Path.Combine(_appSettings?.DataPath ?? string.Empty, "Stations", selectedStation, "Audio", file.Name);

                try
                {
                    await using var fs = new FileStream(path, FileMode.Create);
                    var stream = file.OpenReadStream(maxFileSize);

                    var buffer = new byte[BufferSize];
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
                            var progress = (int)(totalRead * 100 / file.Size);
                            updateProgress(progress);
                            lastUpdate = DateTime.Now;
                        }
                    }

                    await fs.FlushAsync();
                    fs.Close();

                    Log.Information("--- FileManager - LoadFiles() -- File: {Filename} Size: {Size} bytes", file.Name, file.Size);

                    var audioMetadata = await new AudioMetadataService().GetMetadataAsync(path);
                    await SaveAudioMetadata(audioMetadata, file.Name, selectedStation);
                }
                catch (IOException ioEx)
                {
                    Log.Error("++++++ FileManager - LoadFiles() -- File: {Filename} Error: {Error}", file.Name, ioEx.Message);
                }
            }

            await _dbContext.SaveChangesAsync();
            Log.Information("--- FileManager - LoadFiles() -- End - LoadFiles: {Count}", selectedFiles.Count);
        }

        private async Task SaveAudioMetadata(AudioMetadata audioMetadata, string fileName, string selectedStation)
        {
            // Create a new AudioMetadata instance and set its properties
            var audioMetadataForDb = new AudioMetadata
            {
                Filename = fileName,
                Title = audioMetadata.Title,
                Artist = audioMetadata.Artist,
                Duration = audioMetadata.Duration,
                Intro = audioMetadata.Intro,
                Segue = audioMetadata.Segue
            };

            // Find the station with the selected call letters and assign its ID to StationId
            var station = _dbContext.Stations.AsNoTracking().FirstOrDefault(s => s.CallLetters == selectedStation);
            if (station != null)
            {
                audioMetadataForDb.StationId = station.Id;
            }
            else
            {
                Log.Error("Station with call letters {CallLetters} not found", selectedStation);
            }

            _dbContext.AudioMetadatas.Add(audioMetadataForDb);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAudioAsync(AudioMetadata audioMetadata, string selectedStation, string dataPath, AudionixDbContext dbContext, Action getFolderFileList)
        {
            Log.Information("--- FileManager - GetFolderFileList() -- DeleteAudio: " + audioMetadata.Filename);
            File.Delete(Path.Combine(dataPath, "Stations", selectedStation, "Audio", audioMetadata.Filename));

            // Fetch the audioMetadata without tracking it
            var audioMetadataForDb = dbContext.AudioMetadatas.AsNoTracking().FirstOrDefault(am => am.Id == audioMetadata.Id);
            if (audioMetadataForDb != null)
            {
                // Attach the entity to the DbContext before removing it
                dbContext.AudioMetadatas.Attach(audioMetadataForDb);
                dbContext.AudioMetadatas.Remove(audioMetadataForDb);
                await dbContext.SaveChangesAsync();
            }

            getFolderFileList();
            Log.Information("--- FileManager - GetFolderFileList() - End - DeleteAudio: " + audioMetadata.Filename);
        }
    }
}