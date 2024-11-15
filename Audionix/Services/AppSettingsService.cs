using System.Text.Json;
using Audionix.Shared;
using Audionix.Shared.Models;
using MudBlazor;
using Serilog;

namespace Audionix.Services
{
    public class AppSettingsService
    {
        private AppSettings _appSettings;
        private readonly string _configFilePath;

        public AppSettingsService()
        {
            _configFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "Audionix",
                "config.json");
        }

        public async Task<AppSettings> GetOrCreateConfigurationAsync()
        {
            Log.Information($"--- AppSettingsService.cs -- GetOrCreateConfiguration - Getting or creating configuration from {_configFilePath}");

            if (!File.Exists(_configFilePath))
            {
                Log.Information("--- AppSettingsService.cs -- GetOrCreateConfiguration - _configFilePath does not exist.  Creating new configuration");
                _appSettings = CreateDefaultAppSettings();
                await SaveConfigurationAsync(_appSettings.DataPath);
            }
            else
            {
                Log.Information($"--- AppSettingsService.cs -- GetOrCreateConfiguration - _configFilePath exists.  Loading configuration from {_configFilePath}");
                var configJson = await File.ReadAllTextAsync(_configFilePath);
                _appSettings = JsonSerializer.Deserialize<AppSettings>(configJson) ?? new AppSettings();
            }

            Log.Information($"--- AppSettingsService.cs -- GetOrCreateConfiguration - LoggingPath: {_appSettings.LoggingPath}");
            Log.Information("--- AppSettingsService.cs -- GetOrCreateConfiguration - Method Complete - Configuration: {_appSettings}");
            return _appSettings;
        }

        private AppSettings CreateDefaultAppSettings()
        {
            return new AppSettings
            {
                ConfigFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Audionix"),
                DataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Audionix"),
                DatabasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Audionix", "Database", "Audionix.db"),
                IsDatapathSetup = false,
                LoggingPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Audionix", "Logging")
            };
        }

        public async Task SaveConfigurationAsync(string newDataPath)
        {
            Log.Information("--- AppSettingsService.cs -- SaveConfigurationAsync - *STARTING* - Saving configuration to {_configFilePath}");

            // Create the new folder based on the DataPath
            string[] directories = {
                Path.Combine(newDataPath, "Stations")
            };

            foreach (var directory in directories)
            {
                try
                {
                    Directory.CreateDirectory(directory);
                    Log.Information("--- AppSettingsService.cs -- SaveConfigurationAsync - Created directories for new data path at " + directory);
                }
                catch (IOException createError)
                {
                    Log.Error(createError, "--- AppSettingsService.cs -- SaveConfigurationAsync - Error creating directories for new data path");
                }
            }

            // Update AppSettings with the new DataPath
            _appSettings.DataPath = newDataPath;
            _appSettings.IsDatapathSetup = true;

            Log.Information("--- AppSettingsService.cs -- SaveConfigurationAsync - Configuration: {_appSettings}");

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            // Serialize the updated AppSettings object to JSON
            var configJson = JsonSerializer.Serialize(_appSettings, options);

            // Write the updated configuration to the config file
            try
            {
                await File.WriteAllTextAsync(_configFilePath, configJson);
            }
            catch (IOException writeError)
            {
                Log.Error(writeError, "--- AppSettingsService.cs -- SaveConfigurationAsync - Error writing configuration to file");
            }

            Log.Information("--- AppSettingsService.cs -- SaveConfigurationAsync - *FINISHED* - Saved configuration to {_configFilePath}");
        }

        public static void AddStationToDataPath(Station station, AppSettings settings)
        {
            Log.Information("--- AppSettingsService.cs -- AddStationToDataPath - Adding station to data path");
            if (settings.DataPath != null && station.CallLetters != null)
            {
                string stationPath = Path.Combine(settings.DataPath, "Stations", station.CallLetters);

                try
                {
                    Log.Information("--- AppSettingsService.cs -- AddStationToDataPath - Creating directories for station at {stationPath}", stationPath);
                    Directory.CreateDirectory(Path.Combine(stationPath, "Audio"));
                }
                catch (IOException copyError)
                {
                    Log.Error("++++++ AppSettingsService -- AddStationToDataPath - Error creating directories for station: {copyError}", copyError);
                }
            }
        }
    }
}
