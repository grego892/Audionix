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
using static System.Collections.Specialized.BitVector32;

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

        public async Task<List<IBrowserFile>> UploadFiles(IReadOnlyList<IBrowserFile> selectedFiles, string selectedStation, string selectedFolder, List<IBrowserFile> filesToUpload, IList<AudioMetadata> filesInDirectory, Func<Task> loadFiles, Action getFolderFileList, Action<int> updateProgress)
        {
            Log.Information("--- FileManager - UploadFiles() -- UploadFiles: {Count}", selectedFiles.Count);

            // Filter out the files that already exist in the database before adding them to filesToUpload
            var distinctFiles = selectedFiles.Where(file => !_dbContext.AudioMetadatas.Any(f => f.Filename == file.Name)).ToList();
            var existingFiles = selectedFiles.Except(distinctFiles).ToList();

            filesToUpload.Clear();
            filesToUpload.AddRange(distinctFiles);

            await LoadFiles(selectedFiles, selectedStation, selectedFolder, updateProgress);
            getFolderFileList();

            return existingFiles;
        }

        public async Task LoadFiles(IReadOnlyList<IBrowserFile> selectedFiles, string selectedStation, string selectedFolder, Action<int> updateProgress)
        {
            Log.Information("--- FileManager - LoadFiles() -- LoadFiles: {Count}", selectedFiles.Count);

            foreach (var file in selectedFiles)
            {
                var path = Path.Combine(_appSettings?.DataPath ?? string.Empty, "Stations", selectedStation, "Audio", selectedFolder, file.Name);

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

                    var audioMetadataService = new AudioMetadataService(_dbContext);
                    var audioMetadata = await audioMetadataService.GetMetadataAsync(path);
                    await audioMetadataService.SaveAudioMetadata(audioMetadata, file.Name, selectedStation);
                }
                catch (IOException ioEx)
                {
                    Log.Error("++++++ FileManager - LoadFiles() -- File: {Filename} Error: {Error}", file.Name, ioEx.Message);
                }
            }

            await _dbContext.SaveChangesAsync();
            Log.Information("--- FileManager - LoadFiles() -- End - LoadFiles: {Count}", selectedFiles.Count);
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
        public async Task AddFolder(Folder newFolder, Station selectedStation, AudionixDbContext dbContext, ISnackbar snackbar)
        {
            try
            {
                var existingFolder = await dbContext.Folders
                    .FirstOrDefaultAsync(f => f.Name == newFolder.Name && f.StationId == selectedStation.Id);

                if (existingFolder != null)
                {
                    snackbar.Add($"Folder with the same name already exists for this station", Severity.Error);
                }
                else
                {
                    // Create the new folder in the station's path
                    var path = Path.Combine(_appSettings?.DataPath ?? string.Empty, "Stations", selectedStation?.CallLetters ?? string.Empty, "Audio", newFolder?.Name ?? string.Empty);
                    Directory.CreateDirectory(path);

                    if (newFolder != null && selectedStation != null)
                    {
                        newFolder.StationId = selectedStation.Id;
                        dbContext.Folders.Add(newFolder);
                        await dbContext.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "++++++ FileManagerService -- AddFolder() - Error adding folder");
            }
        }

        public async Task RemoveFolder(Folder folder)
        {
            Log.Information("--- FileManager - RemoveFolder() -- Removing Folder: {FolderName}", folder.Name);
            try
            {
                // Get the station associated with the folder
                var station = await _dbContext.Stations.FirstOrDefaultAsync(s => s.Id == folder.StationId);

                if (station != null)
                {
                    // Remove the folder from the file system
                    var path = Path.Combine(_appSettings?.DataPath ?? string.Empty, "Stations", station?.CallLetters ?? string.Empty, "Audio", folder?.Name ?? string.Empty);

                    if (Directory.Exists(path))
                    {
                        Directory.Delete(path, true);
                    }

                    if (folder != null)
                    {
                        // Remove the folder from the database
                        _dbContext.Folders.Remove(folder);
                        await _dbContext.SaveChangesAsync();
                    }

                    Log.Information("--- FileManager - RemoveFolder() -- Folder Removed: {FolderName}", folder.Name);
                }
                else
                {
                    Log.Error("++++++ FileManager - RemoveFolder() -- Station not found for folder: {FolderName}", folder.Name);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "++++++ FileManager - RemoveFolder() -- Error removing folder: {FolderName}", folder.Name);
            }
        }

        public async Task<List<string>> GetFoldersForStation(string stationId)
        {
            return await _dbContext.Folders
                .Where(f => f.StationId == int.Parse(stationId))
                .Select(f => f.Name)
                .ToListAsync();
        }
    }
}