using Microsoft.AspNetCore.SignalR.Client;
using NAudio.Wave.SampleProviders;
using NAudio.Wave;
using Serilog;
using SharedLibrary.Models;
using AudionixAudioServer.DataAccess;
using AudionixAudioServer.Repositories;
using SharedLibrary.Repositories;

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
                Log.Debug("--- AudioPlayer.cs -- PlayAudioAsync() - Starting Task.Run(async () =>)");
                while (!stoppingToken.IsCancellationRequested)
                {
                    Log.Debug("--- AudioPlayer.cs -- PlayAudioAsync() - Starting while NOT Cancelled");
                    var station = await GetStationAsync(stationId, stoppingToken);
                    if (station == null) continue;

                    ProgramLogItem logItem = await GetLogItemAsync(stationId, station, stoppingToken);

                    Log.Debug($"=== AudioPlayer.cs -- PlayAudioAsync() - logItem: {logItem.LogOrderID} - {logItem.Date} - {logItem.Title} - {logItem.Artist}");

                    if (logItem == null) continue;

                    await AdvanceLogNextPlayAsync(logItem);

                    await PlayLogItemAsync(stationId, station, logItem, stoppingToken);
                }
            }, stoppingToken);
        }

        private async Task<Station?> GetStationAsync(Guid stationId, CancellationToken stoppingToken)
        {
            var station = await _unitOfWork.Stations.GetStationByIdAsync(stationId);
            if (station == null)
            {
                Log.Error("+++ AudioPlayer.cs -- GetStationAsync() - station == null");
                await Task.Delay(10000, stoppingToken);
                return null;
            }

            Log.Debug($"--- AudioPlayer.cs -- GetStationAsync() - Station with ID {station.CallLetters} found.");

            if (station.NextPlayDate == null)
            {
                Log.Warning("+++ AudioPlayer.cs -- GetStationAsync() - station.NextPlayDate == null");
                station.NextPlayDate = DateOnly.FromDateTime(DateTime.Today);
                await _unitOfWork.Stations.UpdateStationAsync(station);
                await _unitOfWork.CompleteAsync();
            }

            return station;
        }

        private async Task<ProgramLogItem?> GetLogItemAsync(Guid stationId, Station station, CancellationToken stoppingToken)
        {
            var logItem = await _programLogRepository.GetProgramLogItemAsync(stationId, station.NextPlayId, station.NextPlayDate);
            Log.Debug($"--- AudioPlayer.cs -- GetLogItemAsync() - Retrieved LogItem: {logItem?.LogOrderID} -- {logItem?.Date}");

            return logItem;
        }

        private async Task<ProgramLogItem?> AdvanceLogNextPlayAsync(ProgramLogItem logItem)
        {
            Log.Debug($"--- AudioPlayer.cs -- AdvanceLogNextPlayAsyn() - Method starting.");
            await _programLogRepository.AdvanceLogNextPlayAsync(logItem.StationId);
            return null;
        }

        private async Task PlayLogItemAsync(Guid stationId, Station station, ProgramLogItem logItem, CancellationToken stoppingToken)
        {
            if (!string.IsNullOrEmpty(logItem.Name))
            {
                var audioMetadata = await _unitOfWork.Stations.GetAudioFileByFilenameAsync(logItem.Name);
                Log.Information($"--- AudioPlayer.cs -- PlayLogItemAsync() - Retrieved audioMetadata from log item: {audioMetadata?.Filename}");

                if (audioMetadata != null)
                {
                    var filePath = await GetAudioFilePathAsync(station, audioMetadata, logItem);
                    await PlayAudioFileAsync(stationId, station, logItem, audioMetadata, filePath, stoppingToken);
                }
                else
                {
                    Log.Warning($"+++ AudioPlayer.cs -- PlayLogItemAsync() - No AudioMetadata found for LogItem Name: {logItem.Name}");
                }
            }
            else
            {
                Log.Warning("--- AudioPlayer.cs -- PlayLogItemAsync() - LogItem Name is null or empty.");
            }
        }

        private async Task<string> GetAudioFilePathAsync(Station station, AudioMetadata audioMetadata, ProgramLogItem logItem)
        {
            var folderName = audioMetadata.Folder;
            Log.Debug($"--- AudioPlayer.cs -- GetAudioFilePathAsync() - foldername: {folderName}");

            var appSettings = await _unitOfWork.GetAppSettingsDataPathAsync();
            var filePath = Path.Combine(appSettings.DataPath ?? string.Empty, "Stations", station.CallLetters ?? string.Empty, "Audio", folderName ?? string.Empty, logItem.Name);

            Log.Debug($"--- AudioPlayer.cs -- GetAudioFilePathAsync() - FilePath: {filePath}");
            return filePath;
        }

        private async Task PlayAudioFileAsync(Guid stationId, Station station, ProgramLogItem logItem, AudioMetadata audioMetadata, string filePath, CancellationToken stoppingToken)
        {
            using (var audioFile = new AudioFileReader(filePath))
            using (var outputDevice = new WaveOutEvent())
            {
                var fadeOutProvider = new FadeInOutSampleProvider(audioFile.ToSampleProvider(), true);
                outputDevice.Init(fadeOutProvider);

                fadeOutProvider.BeginFadeIn(.1);

                Log.Information($"--- AudioPlayer.cs -- PlayAudioFileAsync() - Starting audio: {logItem.Title} by {logItem.Artist}");
                outputDevice.Play();
                logItem.Status = StatusType.isPlaying;

                await NotifyClientsAsync(logItem);
                await UpdateLogItemStateAsync(logItem);

                //await UpdateStationCurrentPlayingAsync(stationId, station);

                var seguePosition = audioFile.TotalTime - TimeSpan.FromMilliseconds(audioMetadata.Segue);
                Log.Debug($"--- AudioPlayer.cs -- PlayAudioFileAsync() - TotalTime: {audioFile.TotalTime} - {TimeSpan.FromMilliseconds(audioMetadata.Segue)} = seguePosition: {seguePosition}");

                Task? nextAudioTask = null;

                while (outputDevice.PlaybackState == PlaybackState.Playing)
                {
                    await UpdateProgressAsync(logItem, audioFile);

                    if (audioFile.CurrentTime >= seguePosition && nextAudioTask == null || _isPlayNextTriggered)
                    {
                        _isPlayNextTriggered = false;
                        Log.Information($"--- AudioPlayer.cs -- PlayAudioFileAsync() - Reached segue point for audio: {logItem.Title} by {logItem.Artist}");
                        fadeOutProvider.BeginFadeOut(fadeTime);

                        _ = Task.Run(async () =>
                        {
                            await Task.Delay(fadeTime);
                            outputDevice.Stop();
                            Log.Information($"--- AudioPlayer.cs -- PlayAudioFileAsync() - Stopped audio: {logItem.Title} by {logItem.Artist}");
                        });

                        nextAudioTask = PlayAudioAsync(stationId, stoppingToken);
                        Log.Information("--- AudioPlayer.cs -- PlayAudioFileAsync() - Starting nextAudioTask");
                    }

                    if (outputDevice.PlaybackState == PlaybackState.Playing)
                    {
                        await Task.Delay(100, stoppingToken);
                    }
                }

                logItem.Status = StatusType.hasPlayed;
                await NotifyClientsAsync(logItem);
                await UpdateLogItemStateAsync(logItem);

                if (nextAudioTask != null)
                {
                    Log.Debug("--- AudioPlayer.cs -- PlayAudioFileAsync() - nextAudioTask != null. About to await nextAudioTask");
                    await nextAudioTask;
                    Log.Debug("--- AudioPlayer.cs -- PlayAudioFileAsync() - Awaited nextAudioTask");
                }
            }

            Log.Information($"--- AudioPlayer.cs -- PlayAudioFileAsync() - Playing audio: {logItem.Title} by {logItem.Artist}");
        }

        private async Task NotifyClientsAsync(ProgramLogItem logItem)
        {
            try
            {
                await _hubConnection.InvokeAsync("UpdateLogItemState", logItem);
                Log.Debug($"--- AudioPlayer.cs -- NotifyClientsAsync() - _hubConnection.State:{_hubConnection.State}");
            }
            catch (Exception ex)
            {
                Log.Warning($"+++ AudioPlayer.cs -- NotifyClientsAsync() - Failed to notify clients. Error: {ex.Message}");
            }
        }

        private async Task UpdateLogItemStateAsync(ProgramLogItem logItem)
        {
            Log.Debug($"--- AudioPlayer.cs -- UpdateLogItemStateAsync()");
            await _unitOfWork.Stations.UpdateProgramLogItemAsync(logItem);
            await _unitOfWork.CompleteAsync();
        }

        //private async Task UpdateStationCurrentPlayingAsync(Guid stationId, Station station)
        //{
        //    Log.Debug($"--- AudioPlayer.cs -- PlayAudioAsync() - station.CurrentPlaying WAS: {station.CurrentPlayingId} - {station.CurrentPlayingDate}");
        //    await _programLogRepository.CopyNextPlayToCurrentPlayingAsync(stationId);
        //    Log.Debug($"--- AudioPlayer.cs -- PlayAudioAsync() - station.CurrentPlaying ISNOW: {station.CurrentPlayingId} - {station.CurrentPlayingDate}");

        //    Log.Debug($"--- AudioPlayer.cs -- PlayAudioAsync() - station.NextPlay ISNOW: {station.NextPlayId} - {station.NextPlayDate}");
        //    await _unitOfWork.Stations.UpdateStationAsync(station);
        //    await _unitOfWork.CompleteAsync();
        //}

        private async Task UpdateProgressAsync(ProgramLogItem logItem, AudioFileReader audioFile)
        {
            double currentTime = audioFile.CurrentTime.TotalMilliseconds;
            double totalTime = audioFile.TotalTime.TotalMilliseconds;

            try
            {
                if (_hubConnection.State == HubConnectionState.Connected)
                {
                    await _hubConnection.InvokeAsync("UpdateProgress", logItem.LogOrderID, logItem.Date,currentTime, totalTime);
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"+++ AudioPlayer.cs -- UpdateProgressAsync() - Failed to update progress. Error: {ex.Message}");
            }
        }
    }
}
