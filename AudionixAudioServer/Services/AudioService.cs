using Microsoft.AspNetCore.SignalR.Client;
using Serilog;
using SharedLibrary.Models;
using SharedLibrary.Repositories;

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
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _stationRepository = stationRepository ?? throw new ArgumentNullException(nameof(stationRepository));
            _programLogRepository = programLogRepository ?? throw new ArgumentNullException(nameof(programLogRepository));

            var hubUrl = configuration.GetValue<string>("SignalR:HubUrl");
            if (string.IsNullOrEmpty(hubUrl))
            {
                throw new ArgumentException("SignalR HubUrl is not configured.");
            }

            Log.Debug("--- AudioService -- AudioService() - HubUrl: {HubUrl}", hubUrl);

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(hubUrl, options =>
                {
                    options.HttpMessageHandlerFactory = (message) =>
                    {
                        if (message is HttpClientHandler clientHandler)
                        {
                            // Return a handler that will ignore SSL certificate errors for localhost
                            clientHandler.ServerCertificateCustomValidationCallback +=
                                (sender, certificate, chain, sslPolicyErrors) =>
                                {
                                    if (sender is HttpRequestMessage requestMessage &&
                                        requestMessage.RequestUri != null &&
                                        requestMessage.RequestUri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
                                    {
                                        return true;
                                    }
                                    return sslPolicyErrors == System.Net.Security.SslPolicyErrors.None;
                                };
                        }
                        return message;
                    };
                })
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
                await RetryConnectionAsync();
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
                    Log.Error("--- AudioService -- RetryConnectionAsync() - Failed to connect to the server. Retrying in 5 seconds... Error: {Error}", ex.Message);
                    await Task.Delay(5000);
                }
            }
        }

        public Task PlayAudioAsync(Guid stationId, CancellationToken stoppingToken)
        {
            if (stationId == Guid.Empty)
            {
                throw new ArgumentException("Invalid station ID.");
            }

            return _audioPlayer.PlayAudioAsync(stationId, stoppingToken);
        }

        public async Task PlayNextAudioAsync(Guid stationId)
        {
            if (stationId == Guid.Empty)
            {
                throw new ArgumentException("Invalid station ID.");
            }

            _audioPlayer.TriggerPlayNext();
            Log.Information($"--- AudioService -- PlayNextAudioAsync() - Playing next audio for station: {stationId}");
        }
    }
}
