using Audionix.Models;
using Audionix.Repositories;
using Audionix.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor;
using Serilog;
using static MudBlazor.CategoryTypes;

namespace Audionix.Components.Pages.Studio
{
    public partial class Studio : IDisposable
    {
        private bool _openMakenextDrawer = false;
        private bool _openInsertDrawer = false;
        private bool _openDeleteDrawer = false;
        private AudioMetadata? selectedAudioFile;
        private ProgramLogItem? selectedLogItem;
        private string? _selectedAudioFolder;
        private HubConnection? _hubConnection;

        public List<ProgramLogItem> ProgramLog = new();
        public IEnumerable<AudioMetadata> AudioFiles = new List<AudioMetadata>();
        public List<AudioMetadata> DraggedAudioFiles = new();
        public List<Folder> Folders = new();
        private Folder? selectedFolder = null;

        [Inject] private AppStateService? AppStateService { get; set; }
        [Inject] private IStationRepository? StationRepository { get; set; }
        [Inject] private IAudioMetadataRepository? AudioMetadataRepository { get; set; }
        [Inject] private IProgramLogRepository? ProgramLogRepository { get; set; }

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

            _hubConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5298/progressHub")
                .Build();

            _hubConnection.On<int, double, double>("ReceiveProgress", (logOrderId, currentTime, totalTime) =>
            {
                var logItem = ProgramLog.FirstOrDefault(item => item.LogOrderID == logOrderId);
                if (logItem != null)
                {
                    logItem.Progress = (currentTime / totalTime) * 100;
                    if (currentTime == totalTime)
                    {
                        logItem.States = StatesType.hasPlayed;
                    }
                    InvokeAsync(StateHasChanged);
                }
            });

            // Add handler for SongStopped event
            _hubConnection.On<int>("SongStopped", (logOrderId) =>
            {
                var logItem = ProgramLog.FirstOrDefault(item => item.LogOrderID == logOrderId);
                if (logItem != null)
                {
                    logItem.States = StatesType.hasPlayed;
                    Log.Information("--- Studio - OnInitializedAsync() -- SongStopped received for LogOrderID: {LogOrderID}", logOrderId);
                    InvokeAsync(StateHasChanged);
                }
            });

            await _hubConnection.StartAsync();

        }

        private async void HandleStationChanged(object? sender, EventArgs e)
        {
            if (AppStateService?.station != null)
            {
                await LoadFolders();
                await LoadTodaysLog();
                StateHasChanged();
            }
        }

        private async void PlayNext()
        {
            if (_hubConnection != null)
            {
                await _hubConnection.InvokeAsync("PlayNextAudio", AppStateService?.station?.StationId ?? Guid.Empty);
            }
        }

        private void ToggleMakeNextDrawer()
        {
            _openMakenextDrawer = !_openMakenextDrawer;
        }

        private async void StopAudio()
        {
            if (_hubConnection != null)
            {
                await _hubConnection.InvokeAsync("StopAudio", AppStateService?.station?.StationId ?? Guid.Empty);
            }
        }

        private void ToggleInsertDrawer()
        {
            _openInsertDrawer = !_openInsertDrawer;
        }

        private void ToggleDeleteDrawer()
        {
            _openDeleteDrawer = !_openDeleteDrawer;
        }

        private async Task LoadFolders()
        {
            if (AppStateService?.station != null && StationRepository != null)
            {
                Folders = await StationRepository.GetFoldersForStationAsync(AppStateService.station.StationId);
            }
            else
            {
                Folders = new List<Folder>();
            }
        }

        private async Task LoadAudioFiles()
        {
            if (selectedFolder != null && AudioMetadataRepository != null)
            {
                AudioFiles = await AudioMetadataRepository.GetAudioFilesAsync();
            }
        }

        private async Task OnFolderChanged(Folder folder)
        {
            selectedFolder = folder;
            await LoadAudioFiles();
        }

        private async Task LoadTodaysLog()
        {
            Log.Information("--- Studio - LoadTodaysLog() -- Loading Today's Log");

            if (AppStateService?.station != null && ProgramLogRepository != null)
            {
                ProgramLog = await ProgramLogRepository.GetProgramLogItemsAsync(AppStateService.station.StationId);
            }
            else
            {
                ProgramLog = new List<ProgramLogItem>();
            }
        }

        private async Task SelectAudioFile(AudioMetadata audioFile)
        {
            selectedAudioFile = audioFile;
            _openInsertDrawer = false;

            if (AppStateService?.station != null && ProgramLogRepository != null)
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
        }

        private bool IsAudioFileSelected => selectedAudioFile != null;

        public async Task AddSelectedAudioToLog(int index, ProgramLogItem logItem)
        {
            if (selectedAudioFile != null && ProgramLogRepository != null && AppStateService?.station != null)
            {
                // Shift existing items' LogID
                await ProgramLogRepository.ShiftLogItemsDownAsync(AppStateService.station.StationId, index);

                var newLogItem = new ProgramLogItem
                {
                    Title = selectedAudioFile.Title,
                    Artist = selectedAudioFile.Artist,
                    Name = selectedAudioFile.Filename,
                    Description = selectedAudioFile.Artist,
                    Category = selectedAudioFile.Category,
                    Progress = 0.0,
                    TimeScheduled = TimeOnly.FromDateTime(DateTime.Now),
                    TimePlayed = TimeOnly.FromDateTime(DateTime.Now),
                    Status = "ready",
                    StationId = logItem.StationId,
                    LogOrderID = index,
                    States = StatesType.notPlayed
                };

                await ProgramLogRepository.AddProgramLogItemAsync(newLogItem);
                await LoadTodaysLog();

                // Update the UI without reloading the entire log
                StateHasChanged();
                selectedAudioFile = null;
            }
        }

        public async Task MakeNextSelectedLogItem(ProgramLogItem logItem)
        {
            await Task.Delay(1);
        }

        public async Task StopSelectedLogItem(ProgramLogItem logItem)
        {
            await Task.Delay(1);
        }

        public async Task InsertSelectedLogItem(ProgramLogItem logItem)
        {

        }

        public async Task DeleteSelectedLogItem(ProgramLogItem logItem)
        {
            if (ProgramLogRepository != null)
            {
                await ProgramLogRepository.RemoveProgramLogItemAsync(logItem);
                await ProgramLogRepository.ShiftLogItemsUpAsync(logItem.StationId, logItem.LogOrderID);
                ProgramLog.Remove(logItem);
                _openDeleteDrawer = false;
            }
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
        }
    }
}
