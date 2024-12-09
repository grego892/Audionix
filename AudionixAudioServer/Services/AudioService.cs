﻿using AudionixAudioServer.DataAccess;
using AudionixAudioServer.Repositories;
using Microsoft.AspNetCore.SignalR.Client;
using Serilog;
using SharedLibrary.Models;
using SharedLibrary.Repositories;
using Microsoft.Extensions.Configuration;

namespace AudionixAudioServer.Services
{
    public class AudioService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStationRepository _stationRepository;
        private readonly IProgramLogRepository _programLogRepository;
        private readonly AudioPlayer _audioPlayer;
        private HubConnection _hubConnection;

        public AudioService(IUnitOfWork unitOfWork, IStationRepository stationRepository, IProgramLogRepository programLogRepository, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _stationRepository = stationRepository;
            _programLogRepository = programLogRepository;

            var hubUrl = configuration.GetValue<string>("SignalR:HubUrl");

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(hubUrl)
                .ConfigureLogging(logging =>
                {
                    logging.SetMinimumLevel(LogLevel.Debug);
                    logging.AddConsole();
                })
                .WithAutomaticReconnect() // Enable automatic reconnection
                .Build();

            _audioPlayer = new AudioPlayer(_unitOfWork, _stationRepository, _hubConnection, _programLogRepository);

            // Register the handler for ReceiveProgress
            _hubConnection.On<int, DateOnly, double, double>("ReceiveProgress", (logOrderID, logOrderDate, currentTime, totalTime) => { });

            _hubConnection.On<ProgramLogItem>("UpdateLogItemState", (logItem) =>
            {
                Log.Debug("--- AudioService -- AudioService() - Received UpdateLogItem.");
            });

            // Handle reconnection events
            _hubConnection.Reconnecting += error =>
            {
                Log.Warning("Connection lost. Attempting to reconnect...");
                return Task.CompletedTask;
            };

            _hubConnection.Reconnected += connectionId =>
            {
                Log.Information("Reconnected to the server. ConnectionId: {ConnectionId}", connectionId);
                return Task.CompletedTask;
            };

            _hubConnection.Closed += async error =>
            {
                Log.Error("Connection closed. Trying to restart the connection...");
                _ = RetryConnectionAsync();
            };

            _hubConnection.On<Guid>("PlayNextAudio", async (stationId) =>
            {
                await PlayNextAudioAsync(stationId);
            });

            _ = RetryConnectionAsync();
        }

        private async Task RetryConnectionAsync()
        {
            while (true)
            {
                try
                {
                    await _hubConnection.StartAsync();
                    Log.Information("Connected to the server.");
                    break;
                }
                catch (Exception ex)
                {
                    Log.Error("Failed to connect to the server. Retrying in 5 seconds... Error: {Error}", ex.Message);
                    await Task.Delay(5000);
                }
            }
        }

        public Task PlayAudioAsync(Guid stationId, CancellationToken stoppingToken)
        {
            return _audioPlayer.PlayAudioAsync(stationId, stoppingToken);
        }

        public async Task PlayNextAudioAsync(Guid stationId)
        {
            _audioPlayer.TriggerPlayNext();
            Log.Information($"--- AudioService -- PlayNextAudioAsync() - Playing next audio for station: {stationId}");
        }
    }
}
