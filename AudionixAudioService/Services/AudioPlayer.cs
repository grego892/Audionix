using AudionixAudioServer.DataAccess;
using AudionixAudioServer.Repositories;
using Microsoft.AspNetCore.SignalR.Client;
using NAudio.Wave.SampleProviders;
using NAudio.Wave;
using Serilog;
using AudionixAudioServer.Models;
using System.Threading;
using System.Threading.Tasks;

namespace AudionixAudioServer.Services
{
    public class AudioPlayer
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStationRepository _stationRepository;
        private readonly IProgramLogRepository _programLogRepository;
        private readonly HubConnection _hubConnection;
        private readonly int fadeTime = 1500;
        private bool _isPlayNextTriggered = false;
        private List<ProgramLogItem> _programLogItems = new();

        public AudioPlayer(IUnitOfWork unitOfWork, IStationRepository stationRepository, HubConnection hubConnection, IProgramLogRepository programLogRepository)
        {
            _unitOfWork = unitOfWork;
            _stationRepository = stationRepository;
            _hubConnection = hubConnection;
            _programLogRepository = programLogRepository;
        }

        public void TriggerPlayNext()
        {
            _isPlayNextTriggered = true;
        }

        public Task PlayAudioAsync(Guid stationId, CancellationToken stoppingToken)
        {
            Log.Debug("--- AudioPlayer.cs -- PlayAudioAsync() - Starting PlayAudioAsync()");

            return Task.Run(async () =>
            {
                Log.Debug("--- AudioPlayer.cs -- PlayAudioAsync() - Inside Task.Run(async () =>)");
                while (!stoppingToken.IsCancellationRequested)
                {
                    Log.Debug("--- AudioPlayer.cs -- PlayAudioAsync() - Inside while (!stoppingToken.IsCancellationRequested)");
                    var station = await _stationRepository.GetStationByIdAsync(stationId);

                    if (station == null)
                    {
                        Log.Error("+++ AudioPlayer.cs -- PlayAudioAsync() - station == null");
                        await Task.Delay(10000, stoppingToken);
                        return;
                    }

                    Log.Debug($"+++ AudioPlayer.cs -- PlayAudioAsync() - Station with ID {station.CallLetters} found.");

                    _programLogItems = await _programLogRepository.GetProgramLogItemsAsync(stationId, station.NextPlayId, station.NextPlayDate);

                    Log.Debug($"--- AudioPlayer.cs -- PlayAudioAsync() - _programLogItems: {_programLogItems}");

                    if (_programLogItems == null || !_programLogItems.Any())
                    {
                        Log.Error($"+++ AudioPlayer.cs -- PlayAudioAsync() - No ProgramLogItem found with LogOrderID: {station.NextPlayId}");
                        station.NextPlayId += 1;
                        await _stationRepository.UpdateStationAsync(station);
                        await _unitOfWork.CompleteAsync();
                        continue;
                    }

                    foreach (var logItem in _programLogItems)
                    {
                        if (!string.IsNullOrEmpty(logItem.Name))
                        {
                            var audioMetadata = await _stationRepository.GetAudioFileByFilenameAsync(logItem.Name);

                            Log.Information($"--- AudioPlayer.cs -- PlayAudioAsync() - AudioMetadata.Title: {audioMetadata?.Title}");

                            if (audioMetadata != null)
                            {
                                var folderName = audioMetadata.Folder;
                                Log.Debug($"--- AudioPlayer.cs -- PlayAudioAsync() - foldername: {folderName}");

                                var appSettings = await _stationRepository.GetAppSettingsDataPathAsync();
                                var filePath = Path.Combine(appSettings.DataPath ?? string.Empty, "Stations", station.CallLetters ?? string.Empty, "Audio", folderName ?? string.Empty, logItem.Name);

                                Log.Debug($"--- AudioPlayer.cs -- PlayAudioAsync() - FilePath: {filePath}");

                                using (var audioFile = new AudioFileReader(filePath))
                                using (var outputDevice = new WaveOutEvent())
                                {
                                    var fadeOutProvider = new FadeInOutSampleProvider(audioFile.ToSampleProvider(), true);
                                    outputDevice.Init(fadeOutProvider);

                                    fadeOutProvider.BeginFadeIn(.1);

                                    Log.Information($"--- AudioPlayer.cs -- PlayAudioAsync() - Starting audio: {logItem.Title} by {logItem.Artist}");
                                    outputDevice.Play();
                                    logItem.States = StatesType.isPlaying;

                                    // Notify clients about the state change
                                    try
                                    {
                                        Log.Debug($"--- AudioPlayer.cs -- PlayAudioAsync() - {logItem.Name}");
                                        await _hubConnection.InvokeAsync("UpdateLogItemState", logItem);
                                        Log.Debug($"--- AudioPlayer.cs -- PlayAudioAsync() - {_hubConnection.State}");
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.Warning($"+++ AudioPlayer.cs -- PlayAudioAsync() - Failed to notify clients. Error: {ex.Message}");
                                    }
                                    await _stationRepository.UpdateProgramLogItemAsync(logItem);
                                    await _unitOfWork.CompleteAsync();

                                    // Update the station's CurrentPlaying and NextPlay properties
                                    Log.Debug($"--- AudioPlayer.cs -- PlayAudioAsync() - station.CurrentPlaying WAS: ID: {station.CurrentPlayingId} - Date: {station.CurrentPlayingDate}");
                                    station.CurrentPlayingId = logItem.LogOrderID;
                                    Log.Debug($"--- AudioPlayer.cs -- PlayAudioAsync() - station.CurrentPlaying ISNOW: ID: {station.CurrentPlayingId} - Date: {station.CurrentPlayingDate}");

                                    // Check if NextPlay is the highest LogOrderID for the date
                                    var highestLogOrderID = _programLogItems.Max(item => item.LogOrderID);
                                    if (station.NextPlayId >= highestLogOrderID)
                                    {
                                        station.NextPlayDate = station.NextPlayDate.AddDays(1);
                                        station.NextPlayId = 1;
                                    }
                                    else
                                    {
                                        station.NextPlayId += 1;
                                    }

                                    Log.Debug($"--- AudioPlayer.cs -- PlayAudioAsync() - station.NextPlay ISNOW: {station.NextPlayId}");
                                    await _stationRepository.UpdateStationAsync(station);
                                    await _unitOfWork.CompleteAsync();

                                    var seguePosition = audioFile.TotalTime - TimeSpan.FromMilliseconds(audioMetadata.Segue);
                                    Log.Debug($"--- AudioPlayer.cs -- PlayAudioAsync() - TotalTime: {audioFile.TotalTime} - {TimeSpan.FromMilliseconds(audioMetadata.Segue)} = seguePosition: {seguePosition}");

                                    Task? nextAudioTask = null;

                                    while (outputDevice.PlaybackState == PlaybackState.Playing)
                                    {
                                        // Update song progress
                                        double currentTime = audioFile.CurrentTime.TotalMilliseconds;
                                        double totalTime = audioFile.TotalTime.TotalMilliseconds;

                                        try
                                        {
                                            if (_hubConnection.State == HubConnectionState.Connected)
                                            {
                                                await _hubConnection.InvokeAsync("UpdateProgress", logItem.LogOrderID, currentTime, totalTime);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Log.Warning($"+++ AudioPlayer.cs -- PlayAudioAsync() - Failed to update progress. Error: {ex.Message}");
                                        }

                                        // Check if segue is reached
                                        if (audioFile.CurrentTime >= seguePosition && nextAudioTask == null || _isPlayNextTriggered)
                                        {
                                            _isPlayNextTriggered = false;
                                            Log.Information($"--- AudioPlayer.cs -- PlayAudioAsync() - Reached segue point for audio: {logItem.Title} by {logItem.Artist}");
                                            fadeOutProvider.BeginFadeOut(fadeTime);

                                            // Start a task to stop the output device after fadeTime
                                            _ = Task.Run(async () =>
                                            {
                                                await Task.Delay(fadeTime);
                                                outputDevice.Stop();
                                                Log.Information($"--- AudioPlayer.cs -- PlayAudioAsync() - Stopped audio: {logItem.Title} by {logItem.Artist}");
                                            });

                                            // Start playing the next audio
                                            nextAudioTask = PlayAudioAsync(stationId, stoppingToken);
                                            Log.Information("--- AudioPlayer.cs -- PlayAudioAsync() - Starting nextAudioTask");
                                        }

                                        if (outputDevice.PlaybackState == PlaybackState.Playing)
                                        {
                                            await Task.Delay(100, stoppingToken);
                                        }
                                    }

                                    // Update the log item state to hasPlayed
                                    logItem.States = StatesType.hasPlayed;
                                    try
                                    {
                                        await _hubConnection.InvokeAsync("UpdateLogItemState", logItem);
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.Warning($"+++ AudioPlayer.cs -- PlayAudioAsync() - Failed to notify clients. Error: {ex.Message}");
                                    }
                                    await _stationRepository.UpdateProgramLogItemAsync(logItem);
                                    await _unitOfWork.CompleteAsync();

                                    // Wait for the next audio task to complete if it was started
                                    if (nextAudioTask != null)
                                    {
                                        Log.Debug("--- AudioPlayer.cs -- PlayAudioAsync() - nextAudioTask != null. About to await nextAudioTask");
                                        await nextAudioTask;
                                        Log.Debug("--- AudioPlayer.cs -- PlayAudioAsync() - Awaited nextAudioTask");
                                    }
                                }

                                Log.Information($"--- AudioPlayer.cs -- PlayAudioAsync() - Playing audio: {logItem.Title} by {logItem.Artist}");
                            }
                            else
                            {
                                Log.Warning($"+++ AudioPlayer.cs -- PlayAudioAsync() - No AudioMetadata found for LogItem Name: {logItem.Name}");
                            }
                        }
                        else
                        {
                            Log.Warning("--- AudioPlayer.cs -- PlayAudioAsync() - logItem Name is null or empty.");
                        }
                    }
                }
            }, stoppingToken);
        }
    }
}
