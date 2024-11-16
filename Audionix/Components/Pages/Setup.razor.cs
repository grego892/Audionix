using Microsoft.AspNetCore.Components;
using Serilog;
using Audionix.Services;
using Audionix.Shared;
using System.ServiceProcess;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using Audionix.Shared.Models;
using NAudio.CoreAudioApi;

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
        private AudioDevice? selectedAudioDevice = new AudioDevice(); // Initialize to avoid warning
        private AppSettings? AppConfig { get; set; }
        [Inject] private AppSettingsService AppSettingsService { get; set; } = null!;
        [Inject] ISnackbar? Snackbar { get; set; }
        [Inject] private FileManagerService FileManagerService { get; set; } = null!;


        public string DataPath
        {
            get => AppSettings?.DataPath ?? string.Empty;
            set
            {
                if (AppSettings != null)
                {
                    if (IsValidPath(value))
                    {
                        AppSettings.DataPath = value;
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
            AppSettings = await AppSettingsService.GetOrCreateConfigurationAsync();
            oldDataPath = AppSettings.DataPath;
            stations = await DbContext.Stations.ToListAsync();
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

        private void AddStation()
        {
            Log.Information("--- Setup - AddStation() -- Begin");
            try
            {
                // Check if there are any stations in the database
                int maxSortOrder = DbContext.Stations.Any() ? DbContext.Stations.Max(s => s.StationSortOrder) : 0;
                newStation.StationSortOrder = maxSortOrder + 1;

                // Ensure the selected AudioDeviceId is valid
                if (selectedAudioDevice == null)
                {
                    Log.Error("++++++ Setup - AddStation() -- No AudioDevice selected");
                    Snackbar?.Add("No Audio Device selected.", Severity.Error);
                    return;
                }

                newStation.AudioDeviceId = selectedAudioDevice.Id;
                newStation.AudioDevice = selectedAudioDevice;

                DbContext.Stations.Add(newStation);
                DbContext.SaveChanges();
                if (AppSettings != null)
                {
                    AppSettingsService.AddStationToDataPath(newStation, AppSettings);
                }
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

        private void RemoveStation(Station station)
        {
            Log.Information("--- Setup - RemoveStation() -- Removing Station");
            try
            {
                DbContext.Stations.Remove(station);
                DbContext.SaveChanges();

                // Reorder the remaining stations to ensure StationSortOrder values are consecutive
                var stations = DbContext.Stations.OrderBy(s => s.StationSortOrder).ToList();
                for (int i = 0; i < stations.Count; i++)
                {
                    stations[i].StationSortOrder = i + 1;
                }
                DbContext.SaveChanges();

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

        private void SaveEditedStation(Station saveEditedStation)
        {
            Log.Information("--- Setup - SaveEditedStation() -- Saving Edited Station");
            try
            {
                DbContext.Stations.Update(saveEditedStation);
                DbContext.SaveChanges();
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

        public async Task SaveConfigurationAndRestart()
        {
            Log.Information("--- Setup - SaveConfigurationAndRestart() -- Saving Configuration and Restarting");
            Log.Information("--- Setup - SaveConfigurationAndRestart() -- Old Data Path: {OldDataPath}", oldDataPath);
            if (AppSettings != null)
            {
                AppSettings.DataPath = DataPath;
                await AppSettingsService.SaveConfigurationAsync(DataPath);
                Log.Information("--- Setup - SaveConfigurationAndRestart() -- New Data Path: {DataPath}", DataPath);
                //await AppSettingsService.MoveDatabaseAndLoggingFoldersAsync(oldDataPath, DataPath);
            }
            RestartService();
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

        private async void AddFolder()
        {
            if (selectedStation != null && Snackbar != null)
            {
                await FileManagerService.AddFolder(newFolder, selectedStation, DbContext, Snackbar);
                newFolder = new Folder();
                LoadFolders();
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
                    LoadFolders();
                }
            }
        }

        private async void LoadFolders()
        {
            if (selectedStation != null)
            {
                folders = await DbContext.Folders.Where(f => f.StationId == selectedStation.StationId).ToListAsync();
            }
        }

        private async void RemoveFolder(Folder folder)
        {
            await FileManagerService.RemoveFolder(folder);
            LoadFolders();
        }

        private void MoveStationUp(Station station)
        {
            var currentOrder = station.StationSortOrder;
            var previousStation = DbContext.Stations
                .Where(s => s.StationSortOrder < currentOrder)
                .OrderByDescending(s => s.StationSortOrder)
                .FirstOrDefault();

            if (previousStation != null)
            {
                station.StationSortOrder = previousStation.StationSortOrder;
                previousStation.StationSortOrder = currentOrder;
                DbContext.SaveChanges();
                StateHasChanged();
            }
        }

        private void MoveStationDown(Station station)
        {
            var currentOrder = station.StationSortOrder;
            var nextStation = DbContext.Stations
                .Where(s => s.StationSortOrder > currentOrder)
                .OrderBy(s => s.StationSortOrder)
                .FirstOrDefault();

            if (nextStation != null)
            {
                station.StationSortOrder = nextStation.StationSortOrder;
                nextStation.StationSortOrder = currentOrder;
                DbContext.SaveChanges();
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
                        Id = Guid.NewGuid(), // Use GUID for unique Id
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

        public string GetAudioDeviceFriendlyName(Guid audioDeviceId)
        {
            Log.Debug("--- Setup.razor.cs - GetAudioDeviceFriendlyName() -- Getting friendly name for device: {DeviceId}", audioDeviceId);

            // Query the database for the AudioDevice with the given audioDeviceId
            var device = DbContext.AudioDevices.FirstOrDefault(ad => ad.Id == audioDeviceId);

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
