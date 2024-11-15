using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Audionix.Models;
using Audionix.Data;
using Microsoft.EntityFrameworkCore;
using Audionix.Services;
using static System.Collections.Specialized.BitVector32;
using NAudio.Wave;

namespace AudionixAudioServer
{
    public class Worker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly AppStateService _appStateService;
        private readonly AppSettings _appSettings;

        public Worker(IServiceProvider serviceProvider, AppStateService appStateService, AppSettings appSettings)
        {
            _serviceProvider = serviceProvider;
            _appStateService = appStateService;
            _appSettings = appSettings;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var audioMetadataService = scope.ServiceProvider.GetRequiredService<AudioMetadataService>();

                    var stations = await dbContext.Stations.ToListAsync();

                    for (var i = 0; i < stations.Count; i++)
                    {
                        var currentPlaying = stations[i].ProgramLogItems?.FirstOrDefault()?.LogOrderID ?? 0;

                        if (currentPlaying == 0)
                        {
                            currentPlaying = 1;
                        }

                        // Cross-reference CurrentPlaying with ProgramLogItem.LogOrderID
                        var logItem = await dbContext.Log
                            .FirstOrDefaultAsync(pl => pl.LogOrderID == currentPlaying);

                        if (logItem != null)
                        {
                            // Ensure logItem.Name is not null
                            if (!string.IsNullOrEmpty(logItem.Name))
                            {
                                // Get the AudioMetadata for the logItem
                                var audioMetadata = await dbContext.AudioFiles
                                    .FirstOrDefaultAsync(am => am.Filename == logItem.Name);

                                if (audioMetadata != null)
                                {
                                    // Get the folder name from the AudioMetadata
                                    var folderName = audioMetadata.Folder;

                                    // Construct the file path
                                    var filePath = Path.Combine(_appSettings.DataPath ?? string.Empty, "Stations", stations[i].CallLetters ?? string.Empty, "Audio", folderName ?? string.Empty, logItem.Name);

                                    Log.Debug("--- Worker.cs -- ExecuteAsync() - FilePath: {FilePath}", filePath);

                                    // Play the audio file
                                    using (var audioFile = new AudioFileReader(filePath))
                                    using (var outputDevice = new WaveOutEvent())
                                    {
                                        outputDevice.Init(audioFile);
                                        outputDevice.Play();

                                        // Wait for playback to finish
                                        while (outputDevice.PlaybackState == PlaybackState.Playing)
                                        {
                                            await Task.Delay(1000, stoppingToken);
                                        }
                                    }
                                }
                                else
                                {
                                    Log.Debug("--- Worker.cs -- ExecuteAsync() - No AudioMetadata found for LogItem Name: {Name}", logItem.Name);
                                }
                            }
                            else
                            {
                                Log.Debug("--- Worker.cs -- ExecuteAsync() - LogItem Name is null or empty.");
                            }
                        }
                        else
                        {
                            Log.Debug("--- Worker.cs -- ExecuteAsync() - No ProgramLogItem found with LogOrderID: {LogOrderID}", currentPlaying);
                        }
                    }

                    await Task.Delay(100000, stoppingToken);
                }
            }
        }
    }
}
