using Microsoft.AspNetCore.Components;
using Serilog;
using Audionix.Services;
using Audionix.Models;
using System.ServiceProcess;
using System.Diagnostics;


namespace Audionix.Components.Pages
{
    public partial class Setup
    {
        private int StationEditing { get; set; } = -1;
        private Station newStation = new();
        private Station tempEditStation = new();
        string? oldDataPath;
        private AppSettings? AppConfig { get; set; }
        [Inject] private AppSettingsService AppSettingsService { get; set; } = null!;

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


        private void EditStationButton(Station editingStation)
        {
            Log.Information("--- Setup - EditStationButton() -- Editing Station");
            tempEditStation = editingStation.DeepCopy();
            StationEditing = editingStation.Id;
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
            StationEditing = -1;
        }
        private void CancelStationEdit(Station editingStation)
        {
            Log.Information("--- Setup - CancelStationEdit() -- Cancelling Station Edit");
            editingStation.Id = tempEditStation.Id;
            editingStation.Slogan = tempEditStation.Slogan;
            editingStation.CallLetters = tempEditStation.CallLetters;
            StationEditing = -1;
        }
        private void RemoveStation(Station station)
        {
            Log.Information("--- Setup - RemoveStation() -- Removing Station");
            try
            {
                DbContext.Stations.Remove(station);
                DbContext.SaveChanges();
                Log.Information("--- Setup - RemoveStation() -- Station Removed");
            }
            catch (Exception ex)
            {
                Log.Error("++++++ Setup - RemoveStation() -- " + ex.Message);
            }
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
    }
}