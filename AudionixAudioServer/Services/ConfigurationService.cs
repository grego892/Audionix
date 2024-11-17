using System.Text.Json;
using Serilog;

namespace AudionixAudioServer.Services
{
    public class ConfigurationService
    {
        private readonly string _configFilePath;
        public AppSettings AppSettings { get; private set; }

        public ConfigurationService()
        {
            _configFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "Audionix",
                "config.json");

            AppSettings = GetOrCreateConfigurationAsync().Result;
        }

        private async Task<AppSettings> GetOrCreateConfigurationAsync()
        {
            Log.Information($"--- ConfigurationService.cs -- GetOrCreateConfiguration - Getting or creating configuration from {_configFilePath}");

            if (!File.Exists(_configFilePath))
            {
                Log.Information("--- ConfigurationService.cs -- GetOrCreateConfiguration - _configFilePath does not exist. Creating new configuration");
                AppSettings = CreateDefaultAppSettings();
                await SaveConfigurationAsync(AppSettings.DataPath);
            }
            else
            {
                Log.Information($"--- ConfigurationService.cs -- GetOrCreateConfiguration - _configFilePath exists. Loading configuration from {_configFilePath}");
                var configJson = await File.ReadAllTextAsync(_configFilePath);
                AppSettings = JsonSerializer.Deserialize<AppSettings>(configJson) ?? new AppSettings();
            }

            Log.Information($"--- ConfigurationService.cs -- GetOrCreateConfiguration - LoggingPath: {AppSettings.LoggingPath}");
            Log.Information($"--- ConfigurationService.cs -- GetOrCreateConfiguration - Method Complete - Configuration: {AppSettings}");
            return AppSettings;
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

        private async Task SaveConfigurationAsync(string newDataPath)
        {
            Log.Information("--- ConfigurationService.cs -- SaveConfigurationAsync - *STARTING* - Saving configuration to {_configFilePath}");

            // Create the new folder based on the DataPath
            string[] directories = {
                Path.Combine(newDataPath, "Stations")
            };

            foreach (var directory in directories)
            {
                try
                {
                    Directory.CreateDirectory(directory);
                    Log.Information("--- ConfigurationService.cs -- SaveConfigurationAsync - Created directories for new data path at " + directory);
                }
                catch (IOException createError)
                {
                    Log.Error(createError, "--- ConfigurationService.cs -- SaveConfigurationAsync - Error creating directories for new data path");
                }
            }

            // Update AppSettings with the new DataPath
            AppSettings.DataPath = newDataPath;
            AppSettings.IsDatapathSetup = true;

            Log.Information("--- ConfigurationService.cs -- SaveConfigurationAsync - Configuration: {AppSettings}");

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            // Serialize the updated AppSettings object to JSON
            var configJson = JsonSerializer.Serialize(AppSettings, options);

            // Write the updated configuration to the config file
            try
            {
                await File.WriteAllTextAsync(_configFilePath, configJson);
            }
            catch (IOException writeError)
            {
                Log.Error(writeError, "--- ConfigurationService.cs -- SaveConfigurationAsync - Error writing configuration to file");
            }

            Log.Information("--- ConfigurationService.cs -- SaveConfigurationAsync - *FINISHED* - Saved configuration to {_configFilePath}");
        }
    }

    public class AppSettings
    {
        public string? ConfigFolder { get; set; }
        public string? DataPath { get; set; }
        public string? DatabasePath { get; set; }
        public bool IsDatapathSetup { get; set; }
        public string? LoggingPath { get; set; }
    }
}
