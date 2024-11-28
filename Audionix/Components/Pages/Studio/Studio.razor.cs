﻿using Audionix.Models;
using Audionix.Repositories;
using Audionix.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor;
using Serilog;

namespace Audionix.Components.Pages.Studio
{
    public partial class Studio : IDisposable
    {
        private bool _open = false;
        private bool _delete = false;
        private AudioMetadata? selectedAudioFile;
        private ProgramLogItem? selectedLogItem;
        private string? _selectedAudioFolder;
        private int songProgress;
        private HubConnection _hubConnection;

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

            _hubConnection.On<int>("ReceiveProgress", (progress) =>
            {
                songProgress = progress;
                InvokeAsync(StateHasChanged);
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

        private void ToggleInsertDrawer()
        {
            _open = !_open;
        }

        private void ToggleDelete()
        {
            _delete = !_delete;
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

        private static Func<ProgramLogItem, int, string> RowStyleFunc => (x, i) =>
        {
            return x.Category switch
            {
                "SONG" => ($"background: {Colors.Indigo.Lighten1}; "),
                "AUDIO" => ($"background: {Colors.Blue.Lighten1}; "),
                "MACRO" => ($"background: {Colors.BlueGray.Lighten1}; "),
                "SPOT" => ($"background: {Colors.Green.Lighten1}; "),
                _ => "background-image: #000000)",
            };
        };

        private async Task SelectAudioFile(AudioMetadata audioFile)
        {
            selectedAudioFile = audioFile;
            _open = false;

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

        private void SelectLogItem(ProgramLogItem logItem)
        {
            selectedLogItem = logItem;
        }

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
                    Category = "AUDIO",
                    Progress = 0.0,
                    TimeScheduled = TimeOnly.FromDateTime(DateTime.Now),
                    TimePlayed = TimeOnly.FromDateTime(DateTime.Now),
                    Status = "PLAYING",
                    StationId = logItem.StationId,
                    LogOrderID = index
                };

                await ProgramLogRepository.AddProgramLogItemAsync(newLogItem);
                await LoadTodaysLog();

                // Update the UI without reloading the entire log
                StateHasChanged();
                selectedAudioFile = null;
            }
        }

        public async Task DeleteSelectedLogItem(ProgramLogItem logItem)
        {
            if (ProgramLogRepository != null)
            {
                await ProgramLogRepository.RemoveProgramLogItemAsync(logItem);
                await ProgramLogRepository.ShiftLogItemsUpAsync(logItem.StationId, logItem.LogOrderID);
                ProgramLog.Remove(logItem);
                _delete = false;
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
