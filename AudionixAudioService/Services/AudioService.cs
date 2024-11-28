using AudionixAudioServer.Data;
using AudionixAudioServer.Models;
using AudionixAudioServer;
using AudionixAudioServer.Repositories;
using AudionixAudioServer.DataAccess;
using Microsoft.EntityFrameworkCore;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Serilog;

namespace AudionixAudioServer.Services
{
    public class AudioService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStationRepository _stationRepository;
        private int fadeTime = 2000;

        public AudioService(IUnitOfWork unitOfWork, IStationRepository stationRepository)
        {
            _unitOfWork = unitOfWork;
            _stationRepository = stationRepository;
        }

        //public async Task<ProgramLogItem?> GetCurrentLogItemAsync(Guid stationId)
        //{
        //    var station = await _stationRepository.GetStationByIdAsync(stationId);
        //    if (station == null)
        //    {
        //        Log.Error("+++ AudioService.cs -- GetCurrentLogItemAsync() - Station with ID {stationId} not found.", stationId);
        //        return null;
        //    }

        //    return station.ProgramLogItems?
        //        .FirstOrDefault(log => log.LogOrderID == station.NextPlay);
        //}

        public Task PlayAudioAsync(Guid stationId, CancellationToken stoppingToken)
        {
            Log.Debug($"--- AudioService.cs -- PlayAudioAsync() - Starting PlayAudioAsync()");

            return Task.Run(async () =>
            {
                Log.Debug($"--- AudioService.cs -- PlayAudioAsync() - Inside Task.Run(async () =>)");
                while (!stoppingToken.IsCancellationRequested)
                {
                    Log.Debug($"--- AudioService.cs -- PlayAudioAsync() - Inside while (!stoppingToken.IsCancellationRequested)");
                    var station = await _stationRepository.GetStationByIdAsync(stationId);

                    if (station == null)
                    {
                        Log.Error($"+++ AudioService.cs -- PlayAudioAsync() - station == null");
                        await Task.Delay(10000, stoppingToken);
                        return;
                    }
                    else
                    {
                        Log.Debug($"+++ AudioService.cs -- PlayAudioAsync() - Station with ID {station.CallLetters} found.");
                    }
                    
                    var logItem = await _stationRepository.GetProgramLogItemAsync(stationId, station.NextPlay);

                    Log.Debug($"--- AudioService.cs -- PlayAudioAsync() - LogItem: {logItem}");

                    if (logItem == null)
                    {
                        Log.Error("+++ AudioService.cs -- PlayAudioAsync() - No ProgramLogItem found with LogOrderID: {LogOrderID}", station.NextPlay);
                        await Task.Delay(10000, stoppingToken);
                        return;
                    }

                    if (!string.IsNullOrEmpty(logItem.Name))
                    {
                        var audioMetadata = await _stationRepository.GetAudioFileByFilenameAsync(logItem.Name);

                        Log.Information($"--- AudioService.cs -- PlayAudioAsync() - AudioMetadata.Title: {audioMetadata?.Title}");

                        if (audioMetadata != null)
                        {
                            var folderName = audioMetadata.Folder;
                            Log.Debug($"--- AudioService.cs -- PlayAudioAsync() - foldername: {folderName}");

                            var appSettings = await _stationRepository.GetAppSettingsDataPathAsync();
                            var filePath = Path.Combine(appSettings.DataPath ?? string.Empty, "Stations", station.CallLetters ?? string.Empty, "Audio", folderName ?? string.Empty, logItem.Name);

                            Log.Debug("--- AudioService.cs -- PlayAudioAsync() - FilePath: {FilePath}", filePath);

                            using (var audioFile = new AudioFileReader(filePath))
                            using (var outputDevice = new WaveOutEvent())
                            {
                                var fadeOutProvider = new FadeInOutSampleProvider(audioFile.ToSampleProvider(), true);
                                outputDevice.Init(fadeOutProvider);

                                fadeOutProvider.BeginFadeIn(.1);

                                Log.Information("--- AudioService.cs -- PlayAudioAsync() - Starting audio: {title} by {artist}", logItem.Title, logItem.Artist);
                                outputDevice.Play();

                                // Update the station's CurrentPlaying and NextPlay properties
                                Log.Debug($"--- AudioService.cs -- PlayAudioAsync() - station.CurrentPlaying WAS: {station.CurrentPlaying}");
                                station.CurrentPlaying = logItem.LogOrderID;
                                Log.Debug($"--- AudioService.cs -- PlayAudioAsync() - station.CurrentPlaying ISNOW: {station.CurrentPlaying}");
                                station.NextPlay += 1;
                                Log.Debug($"--- AudioService.cs -- PlayAudioAsync() - station.NextPlay ISNOW: {station.NextPlay}");
                                await _stationRepository.UpdateStationAsync(station);
                                await _unitOfWork.CompleteAsync();

                                var seguePosition = audioFile.TotalTime - TimeSpan.FromMilliseconds(audioMetadata.Segue);
                                Log.Debug($"--- AudioService.cs -- PlayAudioAsync() - TotalTime: {audioFile.TotalTime} - {TimeSpan.FromMilliseconds(audioMetadata.Segue).ToString()} = seguePosition: {seguePosition}");

                                Task? nextAudioTask = null;

                                while (outputDevice.PlaybackState == PlaybackState.Playing)
                                {
                                    // Check if segue is reached
                                    if (audioFile.CurrentTime >= seguePosition && nextAudioTask == null)
                                    {
                                        Log.Information("--- AudioService.cs -- PlayAudioAsync() - Reached segue point for audio: {title} by {artist}", logItem.Title, logItem.Artist);
                                        fadeOutProvider.BeginFadeOut(fadeTime);

                                        // Start playing the next audio
                                        Log.Information("--- AudioService.cs -- PlayAudioAsync() - Starting audio: {title} by {artist}", logItem.Title, logItem.Artist);
                                        nextAudioTask = PlayAudioAsync(stationId, stoppingToken);
                                    }
                                    await Task.Delay(100, stoppingToken);
                                }

                                outputDevice.Stop();
                                Log.Information("Stopped audio: {title} by {artist}", logItem.Title, logItem.Artist);

                                // Wait for the next audio task to complete if it was started
                                if (nextAudioTask != null)
                                {
                                    Log.Debug("--- AudioService.cs -- PlayAudioAsync() - nextAudioTask != null.  About to await nextAudioTask");
                                    await nextAudioTask;
                                    Log.Debug("--- AudioService.cs -- PlayAudioAsync() - Awaited nextAudioTask");
                                }
                            }

                            Log.Information("Playing audio: {title} by {artist}", logItem.Title, logItem.Artist);
                        }
                        else
                        {
                            Log.Warning("--- AudioService.cs -- PlayAudioAsync() - No AudioMetadata found for LogItem Name: {Name}", logItem.Name);
                        }
                    }
                    else
                    {
                        Log.Warning("--- AudioService.cs -- PlayAudioAsync() - LogItem Name is null or empty.");
                    }
                }
            }, stoppingToken);
        }
    }
}
