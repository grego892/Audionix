using SharedLibrary.Models;
using Audionix.Repositories;
using Audionix.DataAccess;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using Serilog;
using Microsoft.AspNetCore.Components;

namespace Audionix.Services
{
    public class FileManagerService
    {
        private const int BufferSize = 81920; // 80 KB chunks
        long maxFileSize = 2L * 1024 * 1024 * 1024; // 2 GB
        private readonly IStationRepository _stationRepository;
        private readonly IAudioMetadataRepository _audioMetadataRepository;
        private readonly IAppSettingsRepository _appSettingsRepository;
        private readonly IFolderRepository _folderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private AppSettings? appSettings;


        public FileManagerService(IStationRepository stationRepository, IAudioMetadataRepository audioMetadataRepository, IAppSettingsRepository appSettingsRepository, IFolderRepository FolderRepository, IUnitOfWork unitOfWork)
        {
            _stationRepository = stationRepository;
            _audioMetadataRepository = audioMetadataRepository;
            _appSettingsRepository = appSettingsRepository;
            _folderRepository = FolderRepository;
            _unitOfWork = unitOfWork;
        }


        public async Task<List<IBrowserFile>> UploadFiles(IReadOnlyList<IBrowserFile> selectedFiles, Guid selectedStation, string selectedFolder, List<IBrowserFile> filesToUpload, IList<AudioMetadata> filesInDirectory, Action<int> updateProgress)
        {
            Log.Information("--- FileManager - UploadFiles() -- UploadFiles: {Count}", selectedFiles.Count);

            // Filter out the files that already exist in the database before adding them to filesToUpload
            var audioFiles = await _audioMetadataRepository.GetAudioFilesAsync();
            var distinctFiles = selectedFiles.Where(file => !audioFiles.Any(f => f.Filename == file.Name)).ToList();
            var existingFiles = selectedFiles.Except(distinctFiles).ToList();

            filesToUpload.Clear();
            filesToUpload.AddRange(distinctFiles);

            await LoadFiles(distinctFiles, selectedStation, selectedFolder, updateProgress);

            return existingFiles;
        }

        public async Task LoadFiles(IReadOnlyList<IBrowserFile> selectedFiles, Guid selectedStation, string selectedFolder, Action<int> updateProgress)
        {
            Log.Information("--- FileManager - LoadFiles() -- LoadFiles: {Count}", selectedFiles.Count);

            // Fetch the CallLetters for the selectedStation
            var station = await _stationRepository.GetStationByIdAsync(selectedStation);
            if (station == null)
            {
                Log.Error("Station not found for StationId: {StationId}", selectedStation);
                return;
            }
            var selectedStationCallLetters = station.CallLetters;
            appSettings = await _appSettingsRepository.GetAppSettingsAsync();

            if (appSettings == null)
            {
                Log.Error("AppSettings is null");
                return;
            }

            foreach (var file in selectedFiles)
            {
                var path = Path.Combine(appSettings.DataPath ?? string.Empty, "Stations", selectedStationCallLetters, "Audio", selectedFolder, file.Name);

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

                    var audioMetadataService = new AudioMetadataService(_stationRepository, _audioMetadataRepository);
                    var audioMetadata = await audioMetadataService.GetMetadataAsync(path);
                    if (audioMetadata != null)
                    {
                        audioMetadata.Folder = selectedFolder;
                        await audioMetadataService.SaveAudioMetadata(audioMetadata, file.Name, selectedStation);
                    }
                    else
                    {
                        Log.Error("Audio metadata is null for file: {Filename}", file.Name);
                    }
                }
                catch (IOException ioEx)
                {
                    Log.Error("++++++ FileManager - LoadFiles() -- File: {Filename} Error: {Error}", file.Name, ioEx.Message);
                }
            }

            await _unitOfWork.CompleteAsync();
            Log.Information("--- FileManager - LoadFiles() -- End - LoadFiles: {Count}", selectedFiles.Count);
        }


        public async Task DeleteAudioAsync(AudioMetadata audioMetadata, string selectedStation, string dataPath, Func<Task> getFolderFileList)
        {
            Log.Information("--- FileManager - GetFolderFileList() -- DeleteAudio: " + audioMetadata.Filename);
            File.Delete(Path.Combine(dataPath, "Stations", selectedStation, "Audio", audioMetadata.Filename));

            // Fetch the audioMetadata without tracking it
            var audioMetadataForDb = await _audioMetadataRepository.GetAudioFileByIdAsync(audioMetadata.Id);
            if (audioMetadataForDb != null)
            {
                await _audioMetadataRepository.DeleteAudioFileAsync(audioMetadataForDb);
            }

            await getFolderFileList();
            Log.Information("--- FileManager - GetFolderFileList() - End - DeleteAudio: " + audioMetadata.Filename);
        }


        public async Task AddFolder(Folder newFolder, Station selectedStation, ISnackbar snackbar)
        {
            try
            {
                if (newFolder == null)
                {
                    Log.Error("++++++ FileManagerService -- AddFolder() - newFolder is null");
                    snackbar.Add("Folder cannot be null", Severity.Error);
                    return;
                }

                if (selectedStation == null)
                {
                    Log.Error("++++++ FileManagerService -- AddFolder() - selectedStation is null");
                    snackbar.Add("Station cannot be null", Severity.Error);
                    return;
                }

                var existingFolders = await _stationRepository.GetFoldersForStationAsync(selectedStation.StationId);
                var existingFolder = existingFolders.FirstOrDefault(f => f.Name == newFolder.Name);

                if (existingFolder != null)
                {
                    snackbar.Add($"Folder with the same name already exists for this station", Severity.Error);
                }
                else
                {
                    // Ensure appSettings is initialized
                    appSettings = appSettings ?? await _appSettingsRepository.GetAppSettingsAsync();

                    if (appSettings == null)
                    {
                        Log.Error("AppSettings is null");
                        return;
                    }

                    // Create the new folder in the station's path
                    var path = Path.Combine(appSettings.DataPath ?? string.Empty, "Stations", selectedStation.CallLetters ?? string.Empty, "Audio", newFolder.Name ?? string.Empty);
                    Directory.CreateDirectory(path);

                    newFolder.StationId = selectedStation.StationId;
                    await _folderRepository.AddFolderAsync(newFolder);
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
                var station = await _stationRepository.GetStationByIdAsync(folder.StationId);
                appSettings = appSettings ?? await _appSettingsRepository.GetAppSettingsAsync();

                if (appSettings == null)
                {
                    Log.Error("AppSettings is null");
                    return;
                }

                if (station != null)
                {
                    // Remove the folder from the file system
                    var path = Path.Combine(appSettings.DataPath ?? string.Empty, "Stations", station?.CallLetters ?? string.Empty, "Audio", folder?.Name ?? string.Empty);

                    if (Directory.Exists(path))
                    {
                        Directory.Delete(path, true);
                    }

                    if (folder != null)
                    {
                        // Remove the folder from the database
                        await _folderRepository.DeleteFolderAsync(folder);
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

            var folders = await _stationRepository.GetFoldersForStationAsync(stationGuid);
            return folders.Select(f => f.Name).ToList();
        }

        public async Task<List<AudioMetadata>> GetFolderFileList(Guid selectedStation, string selectedFolder)
        {
            Log.Information("--- StationService - GetFolderFileList() -- Start");
            var filesInDirectory = await _audioMetadataRepository.GetAudioFilesAsync();

            var filteredFiles = filesInDirectory
                .Where(am => am.StationId == selectedStation && am.Folder == selectedFolder)
                .ToList();

            Log.Information("--- StationService - GetFolderFileList() -- End - filesInDirectory: {Count}", filteredFiles.Count);
            return filteredFiles;
        }
    }
}