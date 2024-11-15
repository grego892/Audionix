using Audionix.Shared.Data;
using Audionix.Shared.Models;
using AudionixAudioServer.Services;
using Microsoft.EntityFrameworkCore;
using NAudio.Wave;
using Serilog;

namespace AudionixAudioServer
{
    public class AudioService
    {
        private readonly ApplicationDbContext _context;
        private readonly ConfigurationService _configurationService;

        public AudioService(ApplicationDbContext context, ConfigurationService configurationService)
        {
            _context = context;
            _configurationService = configurationService;
        }

        public async Task<ProgramLogItem?> GetCurrentLogItemAsync(Guid stationId)
        {
            var station = await _context.Stations
                .Include(s => s.ProgramLogItems)
                .FirstOrDefaultAsync(s => s.StationId == stationId);

            if (station == null)
            {
                Log.Error("Station with ID {stationId} not found.", stationId);
                return null;
            }

            return station.ProgramLogItems?
                .FirstOrDefault(log => log.LogOrderID == station.CurrentPlaying);
        }

        public async Task PlayAudioAsync(ProgramLogItem logItem, CancellationToken stoppingToken)
        {
            if (logItem != null)
            {
                if (!string.IsNullOrEmpty(logItem.Name))
                {
                    var audioMetadata = await _context.AudioFiles
                        .FirstOrDefaultAsync(am => am.Filename == logItem.Name);

                    if (audioMetadata != null)
                    {
                        var folderName = audioMetadata.Folder;
                        var station = await _context.Stations
                            .FirstOrDefaultAsync(s => s.StationId == audioMetadata.StationId);

                        if (station != null)
                        {
                            var filePath = Path.Combine(_configurationService.AppSettings.DataPath ?? string.Empty, "Stations", station.CallLetters ?? string.Empty, "Audio", folderName ?? string.Empty, logItem.Name);

                            Log.Debug("--- AudioService.cs -- PlayAudioAsync() - FilePath: {FilePath}", filePath);

                            using (var audioFile = new AudioFileReader(filePath))
                            using (var outputDevice = new WaveOutEvent())
                            {
                                outputDevice.Init(audioFile);
                                outputDevice.Play();

                                while (outputDevice.PlaybackState == PlaybackState.Playing)
                                {
                                    await Task.Delay(1000, stoppingToken);
                                }
                            }

                            Log.Information("Playing audio: {title} by {artist}", logItem.Title, logItem.Artist);
                        }
                        else
                        {
                            Log.Error("Station with ID {stationId} not found.", audioMetadata.StationId);
                        }
                    }
                    else
                    {
                        Log.Debug("--- AudioService.cs -- PlayAudioAsync() - No AudioMetadata found for LogItem Name: {Name}", logItem.Name);
                    }
                }
                else
                {
                    Log.Debug("--- AudioService.cs -- PlayAudioAsync() - LogItem Name is null or empty.");
                }
            }
            else
            {
                Log.Debug("--- AudioService.cs -- PlayAudioAsync() - No ProgramLogItem found with LogOrderID: {LogOrderID}", logItem?.LogOrderID);
            }
        }
    }
}
