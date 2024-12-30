using Microsoft.AspNetCore.Components;
using Serilog;
using Audionix.Services;
using System.Diagnostics;
using MudBlazor;
using SharedLibrary.Models;
using NAudio.CoreAudioApi;
using SharedLibrary.Repositories;

namespace Audionix.Components.Pages
{
    public partial class Setup
    {
        private Guid StationEditing { get; set; }
        private Station newStation = new();
        private Station tempEditStation = new();
        string? oldDataPath;
        private Folder newFolder = new();
        private List<Station> stations = new List<Station>();
        private Station? selectedStation;
        private List<AudioDevice> audioDevices = new List<AudioDevice>();
        private List<Folder> folders = new List<Folder>();
        private AudioDevice? selectedAudioDevice;
        private string? databaseErrorMessage;
        [Inject] ISnackbar? Snackbar { get; set; }
        [Inject] private FileManagerService FileManagerService { get; set; } = null!;
        [Inject] private IStationRepository StationRepository { get; set; } = null!;
        [Inject] private AppStateService AppStateService { get; set; } = null!;

        public string DataPath
        {
            get => AppStateService.AppSettings?.DataPath ?? string.Empty;
            set
            {
                if (AppStateService.AppSettings != null)
                {
                    if (IsValidPath(value))
                    {
                        AppStateService.AppSettings.DataPath = value;
                    }
                    else
                    {
                        Log.Warning("--- Setup.razor.cs -- DataPath - Invalid path: {Path}", value);
                    }
                }
            }
        }

        protected override async Task OnInitializedAsync()
        {
            Log.Information("--- Setup.razor.cs - OnInitializedAsync() -- Initializing");
            try
            {
                await AppStateService.LoadAppSettingsAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "+++ Setup.razor.cs - OnInitializedAsync() -- Error loading application settings.");
                Snackbar?.Add("Failed to load application settings.", Severity.Error);
                return;
            }

            if (AppStateService.AppSettings != null)
            {
                oldDataPath = AppStateService.AppSettings.DataPath;
            }
            else
            {
                Log.Error("+++ Setup.razor.cs - OnInitializedAsync() -- appSettings is null");
                Snackbar?.Add("Failed to load application settings.", Severity.Error);
                return;
            }

            stations = await StationRepository.GetStationsAsync();
            audioDevices = GetWasapiOutputDevices();

            if (audioDevices == null || !audioDevices.Any())
            {
                Log.Warning("--- Setup.razor.cs - OnInitializedAsync() -- No audio devices found");
                Snackbar?.Add("No audio devices found.", Severity.Warning);
            }
            else
            {
                Log.Information("--- Setup.razor.cs - OnInitializedAsync() -- Found {Count} audio devices", audioDevices.Count);
            }
        }

        public async Task SaveDataPath()
        {
            Log.Information("--- Setup - SaveConfigurationAndRestart() -- Saving Configuration");
            Log.Information("--- Setup - SaveConfigurationAndRestart() -- Old Data Path: {OldDataPath}", oldDataPath);
            if (AppStateService.AppSettings != null)
            {
                AppStateService.AppSettings.DataPath = DataPath;
                AppStateService.AppSettings.IsDatapathSetup = true;
                await AppStateService.SaveAppSettingsAsync();
                Log.Information("--- Setup - SaveConfigurationAndRestart() -- New Data Path: {DataPath}", DataPath);
            }
            StateHasChanged();
        }

        private bool IsValidPath(string path)
        {
            bool isValid = false;

            if (!string.IsNullOrEmpty(path))
            {
                string fullPath = Path.GetFullPath(path);

                string root = Path.GetPathRoot(path) ?? string.Empty;
                isValid = string.IsNullOrEmpty(root.Trim(new char[] { '\\', '/' })) == false;
                isValid = isValid && fullPath.StartsWith(root);
                isValid = isValid && Path.IsPathRooted(path);
                isValid = isValid && !Path.GetFullPath(path).Contains("~");
            }

            Log.Information("--- Setup.razor.cs -- IsValidPath - {IsValid}", isValid);
            return isValid;
        }

        private async Task AddStationAsync()
        {
            Log.Information("--- Setup - AddStation() -- Begin");
            try
            {
                // Check if there are any stations in the database
                int maxSortOrder = stations.Any() ? stations.Max(s => s.StationSortOrder) : 0;
                newStation.StationSortOrder = maxSortOrder + 1;

                // Ensure the selected AudioDeviceId is valid
                if (selectedAudioDevice == null)
                {
                    Log.Error("++++++ Setup - AddStation() -- No AudioDevice selected");
                    Snackbar?.Add("No Audio Device selected.", Severity.Error);
                    return;
                }

                newStation.AudioDeviceId = selectedAudioDevice.Id; // Use Id instead of DeviceID
                newStation.AudioDevice = selectedAudioDevice;

                await StationRepository.AddStationAsync(newStation);

                StateHasChanged();
                NavigationManager.NavigateTo(NavigationManager.Uri, true);
                Log.Information("--- Setup - AddStation() -- Station Added");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "++++++ Setup - AddStation() -- Error adding station");
            }
            newStation = new Station();

            StateHasChanged();
        }

        private async Task RemoveStationAsync(Station station)
        {
            Log.Information("--- Setup - RemoveStation() -- Removing Station");
            try
            {
                await StationRepository.DeleteStationAsync(station.StationId);

                // Reorder the remaining stations to ensure StationSortOrder values are consecutive
                stations = await StationRepository.GetStationsAsync();
                for (int i = 0; i < stations.Count; i++)
                {
                    stations[i].StationSortOrder = i + 1;
                }

                Log.Information("--- Setup - RemoveStation() -- Station Removed");
            }
            catch (Exception ex)
            {
                Log.Error("++++++ Setup - RemoveStation() -- " + ex.Message);
            }
        }

        private void EditStationButton(Station editingStation)
        {
            Log.Information("--- Setup - EditStationButton() -- Editing Station");
            tempEditStation = editingStation.DeepCopy();
            StationEditing = editingStation.StationId;
        }

        private async Task SaveEditedStationAsync(Station saveEditedStation)
        {
            Log.Information("--- Setup - SaveEditedStation() -- Saving Edited Station");
            try
            {
                await StationRepository.UpdateStationAsync(saveEditedStation);
                Log.Information("--- Setup - SaveEditedStation() -- Station Saved");
            }
            catch (Exception ex)
            {
                Log.Error("++++++ Setup - SaveEditedStation() -- " + ex.Message);
            }
            StationEditing = Guid.Empty;
        }

        private void CancelStationEdit(Station editingStation)
        {
            Log.Information("--- Setup - CancelStationEdit() -- Cancelling Station Edit");
            editingStation.StationId = tempEditStation.StationId;
            editingStation.Slogan = tempEditStation.Slogan;
            editingStation.CallLetters = tempEditStation.CallLetters;
            StationEditing = Guid.Empty;
        }

        public void RestartService()
        {
            try
            {
                const string strCmdText = "/C net stop \"Audionix\"&net start \"Audionix\"";
                Process.Start("CMD.exe", strCmdText);
            }
            catch (Exception ex)
            {
                Log.Error("++++++ Setup - RestartService() -- " + ex.Message);
            }
        }

        private async Task AddFolder()
        {
            if (selectedStation != null && Snackbar != null)
            {
                await FileManagerService.AddFolder(newFolder, selectedStation, Snackbar);
                newFolder = new Folder();
                await LoadFolders();
                StateHasChanged();
            }
        }

        private Station? SelectedStation
        {
            get { return selectedStation; }
            set
            {
                if (selectedStation != value)
                {
                    selectedStation = value;
                    _ = LoadFolders(); // Fix the asynchronous method call
                }
            }
        }

        private async Task LoadFolders()
        {
            if (selectedStation != null)
            {
                folders = await StationRepository.GetFoldersForStationAsync(selectedStation.StationId);
                StateHasChanged();
            }
        }

        private async Task RemoveFolder(Folder folder)
        {
            await FileManagerService.RemoveFolder(folder);
            await LoadFolders();
        }

        private async Task MoveStationUpAsync(Station station)
        {
            var currentOrder = station.StationSortOrder;
            var previousStation = stations
                .Where(s => s.StationSortOrder < currentOrder)
                .OrderByDescending(s => s.StationSortOrder)
                .FirstOrDefault();

            if (previousStation != null)
            {
                station.StationSortOrder = previousStation.StationSortOrder;
                previousStation.StationSortOrder = currentOrder;
                await StationRepository.UpdateStationAsync(station);
                await StationRepository.UpdateStationAsync(previousStation);
                StateHasChanged();
            }
        }

        private async Task MoveStationDownAsync(Station station)
        {
            var currentOrder = station.StationSortOrder;
            var nextStation = stations
                .Where(s => s.StationSortOrder > currentOrder)
                .OrderBy(s => s.StationSortOrder)
                .FirstOrDefault();

            if (nextStation != null)
            {
                station.StationSortOrder = nextStation.StationSortOrder;
                nextStation.StationSortOrder = currentOrder;
                await StationRepository.UpdateStationAsync(station);
                await StationRepository.UpdateStationAsync(nextStation);
                StateHasChanged();
            }
        }

        public List<AudioDevice> GetWasapiOutputDevices()
        {
            var devices = new List<AudioDevice>();
            var enumerator = new MMDeviceEnumerator();

            try
            {
                foreach (var device in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
                {
                    devices.Add(new AudioDevice
                    {
                        DeviceID = device.ID,
                        FriendlyName = device.FriendlyName
                    });
                }

                Log.Information("--- Setup.razor.cs - GetWasapiOutputDevices() -- Found {Count} devices", devices.Count);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "++++++ Setup.razor.cs - GetWasapiOutputDevices() -- Error enumerating audio devices");
            }

            return devices;
        }

        public string GetAudioDeviceFriendlyName(int audioDeviceId)
        {
            Log.Debug("--- Setup.razor.cs - GetAudioDeviceFriendlyName() -- Getting friendly name for device: {DeviceId}", audioDeviceId);

            // Query the database for the AudioDevice with the given audioDeviceId
            var device = audioDevices.FirstOrDefault(ad => ad.Id == audioDeviceId);

            if (device != null)
            {
                Log.Debug("--- Setup.razor.cs - GetAudioDeviceFriendlyName() -- Found device: {Device}", device);
                return device.FriendlyName ?? "Unknown Device";
            }
            else
            {
                Log.Warning("--- Setup.razor.cs - GetAudioDeviceFriendlyName() -- Device not found for Id: {DeviceId}", audioDeviceId);
                return "Unknown Device";
            }
        }
    }
}
