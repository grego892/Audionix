using Audionix.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using Serilog;


namespace Audionix.Services
{
    public class FileManagerService
    {
        private const int BufferSize = 81920; // 80 KB chunks
        long maxFileSize = 2L * 1024 * 1024 * 1024; // 2 GB
        private readonly ApplicationDbContext _dbContext;
        private readonly AppSettings? _appSettings;

        public FileManagerService(ApplicationDbContext dbContext, AppSettings? appSettings)
        {
            _dbContext = dbContext;
            _appSettings = appSettings;
        }

        public async Task<List<IBrowserFile>> UploadFiles(IReadOnlyList<IBrowserFile> selectedFiles, string selectedStation, string selectedFolder, List<IBrowserFile> filesToUpload, IList<AudioMetadata> filesInDirectory, Action<int> updateProgress)
        {
            Log.Information("--- FileManager - UploadFiles() -- UploadFiles: {Count}", selectedFiles.Count);

            // Filter out the files that already exist in the database before adding them to filesToUpload
            var distinctFiles = selectedFiles.Where(file => !_dbContext.AudioFiles.Any(f => f.Filename == file.Name)).ToList();
            var existingFiles = selectedFiles.Except(distinctFiles).ToList();

            filesToUpload.Clear();
            filesToUpload.AddRange(distinctFiles);

            await LoadFiles(distinctFiles, selectedStation, selectedFolder, updateProgress);

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
                    audioMetadata.Folder = selectedFolder;
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

        public async Task DeleteAudioAsync(AudioMetadata audioMetadata, string selectedStation, string dataPath, ApplicationDbContext dbContext, Action getFolderFileList)
        {
            Log.Information("--- FileManager - GetFolderFileList() -- DeleteAudio: " + audioMetadata.Filename);
            File.Delete(Path.Combine(dataPath, "Stations", selectedStation, "Audio", audioMetadata.Filename));

            // Fetch the audioMetadata without tracking it
            var audioMetadataForDb = dbContext.AudioFiles.AsNoTracking().FirstOrDefault(am => am.Id == audioMetadata.Id);
            if (audioMetadataForDb != null)
            {
                // Attach the entity to the DbContext before removing it
                dbContext.AudioFiles.Attach(audioMetadataForDb);
                dbContext.AudioFiles.Remove(audioMetadataForDb);
                await dbContext.SaveChangesAsync();
            }

            getFolderFileList();
            Log.Information("--- FileManager - GetFolderFileList() - End - DeleteAudio: " + audioMetadata.Filename);
        }
        public async Task AddFolder(Folder newFolder, Station selectedStation, ApplicationDbContext dbContext, ISnackbar snackbar)
        {
            try
            {
                var existingFolder = await dbContext.Folders
                    .FirstOrDefaultAsync(f => f.Name == newFolder.Name && f.StationId == selectedStation.StationId);

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
                        newFolder.StationId = selectedStation.StationId;
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
                var station = await _dbContext.Stations.FirstOrDefaultAsync(s => s.StationId == folder.StationId);

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
            if (!Guid.TryParse(stationId, out var stationGuid))
            {
                throw new ArgumentException("Invalid station ID format", nameof(stationId));
            }

            return await _dbContext.Folders
                .Where(f => f.StationId == stationGuid)
                .Select(f => f.Name)
                .ToListAsync();
        }

        public List<AudioMetadata> GetFolderFileList(string selectedStation, string selectedFolder, List<Station>? stations, ApplicationDbContext dbContext)
        {
            Log.Information("--- StationService - GetFolderFileList() -- Start");
            var filesInDirectory = new List<AudioMetadata>();

            if (!string.IsNullOrEmpty(selectedStation) && stations != null)
            {
                var station = stations.FirstOrDefault(s => s.CallLetters == selectedStation);
                if (station != null)
                {
                    filesInDirectory = dbContext.AudioFiles
                        .AsNoTracking()
                        .Where(am => am.StationId == station.StationId && am.Folder == selectedFolder)
                        .ToList();
                }
            }

            Log.Information("--- StationService - GetFolderFileList() -- End - filesInDirectory: {Count}", filesInDirectory.Count);
            return filesInDirectory;
        }
    }
}