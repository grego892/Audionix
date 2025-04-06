using SharedLibrary.Models;
using SharedLibrary.Repositories;
using Audionix.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor;
using Serilog;
using System.Text.RegularExpressions;
using System.Timers;

namespace Audionix.Components.Pages.Studio
{
    public partial class Studio : IDisposable
    {
        private bool _openMakenextDrawer = false;
        private bool _openInsertDrawer = false;
        private bool _openDeleteDrawer = false;
        private AudioMetadata? selectedAudioFile;
        private ProgramLogItem? selectedLogItem;
        private HubConnection? _hubConnection;
        private string timeDifferenceFormatted = string.Empty;
        private System.Timers.Timer? _timer;
        private DateTime _currentStationTime;
        private bool _isRendered = false;
        private bool _isDisposed = false;
        private CancellationTokenSource _cancellationTokenSource = new();

        public List<ProgramLogItem> ProgramLog = new();
        public IEnumerable<AudioMetadata> AudioFiles = new List<AudioMetadata>();
        public List<AudioMetadata> DraggedAudioFiles = new();
        public List<Folder> Folders = new();
        private Folder? selectedFolder = null;

        [Inject] private AppStateService? AppStateService { get; set; }
        [Inject] private IStationRepository? StationRepository { get; set; }
        [Inject] private IAudioMetadataRepository? AudioMetadataRepository { get; set; }
        [Inject] private IProgramLogRepository? ProgramLogRepository { get; set; }
        [Inject] private NavigationManager? NavigationManager { get; set; }
        [Inject] private IConfiguration? Configuration { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Log.Information("--- Studio - OnInitializedAsync() -- Initializing Studio Page");
            if (AppStateService != null)
            {
                AppStateService.OnStationChanged += HandleStationChanged;
                await LoadTodaysLog();
                await LoadFolders();
                StateHasChanged();
            }

            var hubUrl = Configuration?.GetValue<string>("SignalR:HubUrl");

            if (hubUrl == null)
            {
                throw new ArgumentNullException(nameof(hubUrl), "Hub URL cannot be null");
            }

            Log.Debug($"--- Studio - OnInitializedAsync() -- HubUrl: {hubUrl}");

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
                .Build();

            _hubConnection.On<int, DateOnly, double, double>("ReceiveProgress", (logOrderId, logOrderDate, currentTime, totalTime) =>
            {
                var logItem = ProgramLog.FirstOrDefault(item => item.LogOrderID == logOrderId && item.Date == logOrderDate);
                if (logItem != null)
                {
                    logItem.Progress = (currentTime / totalTime) * 100;
                    if (currentTime == totalTime)
                    {
                        logItem.Status = StatusType.hasPlayed;
                    }
                    var timeDifference = (totalTime - currentTime); // Calculate the difference in milliseconds
                    var timeSpan = TimeSpan.FromMilliseconds(timeDifference);
                    timeDifferenceFormatted = timeSpan.ToString(@"hh\:mm\:ss\:ff");

                    // Remove leading zeros and colons
                    timeDifferenceFormatted = Regex.Replace(timeDifferenceFormatted, @"^0+(:0+)*", "").TrimStart(':');

                    InvokeAsync(StateHasChanged);
                }
            });

            _hubConnection.On<ProgramLogItem>("UpdateLogItemState", (updatedLogItem) =>
            {
                var logItem = ProgramLog.FirstOrDefault(item => item.LogOrderID == updatedLogItem.LogOrderID && item.Date == updatedLogItem.Date);
                if (logItem != null)
                {
                    logItem.Status = updatedLogItem.Status;
                    logItem.Progress = updatedLogItem.Progress;
                    InvokeAsync(StateHasChanged);

                    InvokeAsync(ScrollToCurrentPlayingItem);
                }
            });

            await _hubConnection.StartAsync();

            _currentStationTime = DateTime.Now;
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += UpdateClock;
            _timer.Start();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await ScrollToCurrentPlayingItem();
            }

            if (!_isRendered)
            {
                _isRendered = true;
                await ScrollToCurrentPlayingItem();
            }
        }

        private Dictionary<string, SortDefinition<ProgramLogItem>> initialSorts = new()
        {
            { "Date", new SortDefinition<ProgramLogItem>("Date", false, 0, item => item.Date, null) },
            { "LogOrderID", new SortDefinition<ProgramLogItem>("LogOrderID", false, 1, item => item.LogOrderID, null) }
        };

        private async void HandleStationChanged(object? sender, EventArgs e)
        {
            if (_isDisposed) return;

            if (AppStateService?.station != null)
            {
                await LoadFolders();
                await LoadTodaysLog();
                StateHasChanged();
            }
        }

        private async void PlayNext()
        {
            if (_hubConnection != null && !_isDisposed)
            {
                try
                {
                    await _hubConnection.InvokeAsync("PlayNextAudio", AppStateService?.station?.StationId ?? 0);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error while invoking PlayNextAudio");
                }
            }
            _openMakenextDrawer = false;
            _openInsertDrawer = false;
            _openDeleteDrawer = false;
            selectedAudioFile = null;
        }

        private void ToggleMakeNextDrawer()
        {
            _openMakenextDrawer = !_openMakenextDrawer;
            _openInsertDrawer = false;
            _openDeleteDrawer = false;
            selectedAudioFile = null;
        }

        private void ToggleInsertDrawer()
        {
            _openInsertDrawer = !_openInsertDrawer;
            _openMakenextDrawer = false;
            _openDeleteDrawer = false;
            selectedAudioFile = null;
        }

        private void ToggleDeleteDrawer()
        {
            _openDeleteDrawer = !_openDeleteDrawer;
            _openMakenextDrawer = false;
            _openInsertDrawer = false;
            selectedAudioFile = null;
        }

        private async Task LoadFolders()
        {
            if (_isDisposed) return;

            if (AppStateService?.station != null && StationRepository != null)
            {
                try
                {
                    Folders = await StationRepository.GetFoldersForStationAsync(AppStateService.station.StationId);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error while loading folders");
                    Folders = new List<Folder>();
                }
            }
            else
            {
                Folders = new List<Folder>();
            }
        }

        private async Task LoadAudioFiles()
        {
            if (_isDisposed) return;

            if (selectedFolder != null && AudioMetadataRepository != null)
            {
                try
                {
                    AudioFiles = await AudioMetadataRepository.GetAudioFilesAsync();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error while loading audio files");
                    AudioFiles = new List<AudioMetadata>();
                }
            }
        }

        private async Task OnFolderChanged(Folder folder)
        {
            if (_isDisposed) return;

            selectedFolder = folder;
            await LoadAudioFiles();
        }

        private async Task LoadTodaysLog()
        {
            if (_isDisposed) return;

            Log.Information("--- Studio - LoadTodaysLog() -- Loading Today's Log");

            if (AppStateService?.station != null && ProgramLogRepository != null)
            {
                try
                {
                    ProgramLog = await ProgramLogRepository.GetFullProgramLogForStationAsync(AppStateService.station.StationId);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error while loading today's log");
                    ProgramLog = new List<ProgramLogItem>();
                }
            }
            else
            {
                ProgramLog = new List<ProgramLogItem>();
            }
        }

        private async Task SelectAudioFile(AudioMetadata audioFile)
        {
            if (_isDisposed) return;

            selectedAudioFile = audioFile;
            _openInsertDrawer = false;

            if (AppStateService?.station != null && ProgramLogRepository != null)
            {
                try
                {
                    bool hasLogEntries = await ProgramLogRepository.HasLogEntriesAsync(AppStateService.station.StationId);

                    if (!hasLogEntries)
                    {
                        if (selectedLogItem == null)
                        {
                            selectedLogItem = new ProgramLogItem
                            {
                                // Initialize with default values
                                StationId = AppStateService.station.StationId,
                            };
                        }

                        await AddSelectedAudioToLog(1, selectedLogItem);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error while selecting audio file");
                }
            }
        }

        private bool IsAudioFileSelected => selectedAudioFile != null;

        public async Task AddSelectedAudioToLog(int index, ProgramLogItem logItem)
        {
            if (_isDisposed) return;

            if (selectedAudioFile != null && ProgramLogRepository != null && AppStateService?.station != null)
            {
                try
                {
                    // Shift existing items' LogID
                    await ProgramLogRepository.ShiftLogItemsDownAsync(AppStateService.station.StationId, index);

                    var newLogItem = new ProgramLogItem
                    {
                        Title = selectedAudioFile.Title,
                        Artist = selectedAudioFile.Artist,
                        Name = selectedAudioFile.Filename,
                        SongCategory = selectedAudioFile.SongCategory?.SongCategoryName, // Fixed the property name
                        Progress = 0.0,
                        Cue = "AutoStart",
                        Intro = selectedAudioFile.Intro,
                        Segue = selectedAudioFile.Segue,
                        //EventType = selectedAudioFile.EventType,
                        TimeScheduled = logItem.TimeScheduled, // Use the TimeScheduled of the item being shifted down
                        Status = StatusType.notPlayed,
                        StationId = logItem.StationId,
                        LogOrderID = index,
                        Date = logItem.Date // Use the Date of the item being shifted down
                    };

                    await ProgramLogRepository.AddProgramLogItemAsync(newLogItem);
                    await LoadTodaysLog();

                    // Update the UI without reloading the entire log
                    StateHasChanged();
                    selectedAudioFile = null;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error while adding selected audio to log");
                }
            }
        }

        public async Task MakeNextSelectedLogItem(int logOrderID, DateOnly date)
        {
            if (_isDisposed) return;

            if (AppStateService?.station != null && StationRepository != null)
            {
                try
                {
                    await StationRepository.UpdateStationNextPlayAsync(AppStateService.station.StationId, logOrderID, date);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error while making next selected log item");
                }
            }
            _openMakenextDrawer = false;
        }

        public async Task DeleteSelectedLogItem(ProgramLogItem logItem)
        {
            if (_isDisposed) return;

            if (ProgramLogRepository != null)
            {
                try
                {
                    await ProgramLogRepository.RemoveProgramLogItemAsync(logItem);
                    await ProgramLogRepository.ShiftLogItemsUpAsync(logItem.StationId, logItem.LogOrderID);
                    ProgramLog.Remove(logItem);
                    _openDeleteDrawer = false;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error while deleting selected log item");
                }
            }
        }

        private async Task ScrollToCurrentPlayingItem()
        {
            if (_isDisposed) return;

            if (AppStateService?.station == null || StationRepository == null)
            {
                return;
            }

            try
            {
                Station? station = await StationRepository.GetStationByIdAsync(AppStateService.station.StationId);

                if (station == null)
                {
                    return;
                }

                // Order the ProgramLog by Date and LogOrderID
                var orderedProgramLog = ProgramLog
                    .OrderBy(item => item.Date)
                    .ThenBy(item => item.LogOrderID);

                // Find the index of the currently playing item
                var index = orderedProgramLog
                    .Select((item, idx) => new { item, idx })
                    .FirstOrDefault(x => x.item.Date == station.CurrentPlayingDate && x.item.LogOrderID == station.CurrentPlayingId)?.idx ?? -1;

                if (index >= 0)
                {
                    index--;
                }

                int scrollTo = index * 33;

                await ScrollManager.ScrollToAsync(".mud-table-container", 0, scrollTo, ScrollBehavior.Smooth);
                Log.Debug($"--- Studio - ScrollToCurrentPlayingItem() -- Scrolling to CurrentPlayingItem - Index: {index} -- ScrollTo: {scrollTo}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while scrolling to current playing item");
            }
        }

        private string FormatTimeSpan(TimeSpan timeSpan)
        {
            string formatted = timeSpan.ToString(@"hh\:mm\:ss");
            return Regex.Replace(formatted, @"^0+(:0+)*", "").TrimStart(':');
        }

        private void UpdateClock(object? sender, ElapsedEventArgs e)
        {
            if (_isDisposed) return;

            _currentStationTime = DateTime.Now;
            InvokeAsync(StateHasChanged); // Request UI update
        }

        public void Dispose()
        {
            if (AppStateService != null)
            {
                AppStateService.OnStationChanged -= HandleStationChanged;
            }

            if (_hubConnection != null)
            {
                _hubConnection.StopAsync().GetAwaiter().GetResult();
                _hubConnection.DisposeAsync().GetAwaiter().GetResult();
            }

            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
            }

            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();

            _isDisposed = true;
        }
    }
}

