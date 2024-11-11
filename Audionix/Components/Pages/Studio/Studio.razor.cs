using MudBlazor;
using Serilog;
using Audionix.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components;


namespace Audionix.Components.Pages.Studio
{
    public partial class Studio : IDisposable
    {
        private bool _open = false;
        private bool _delete = false;
        private AudioMetadata? selectedAudioFile;
        private ProgramLogItem? selectedLogItem;
        private string? _selectedAudioFolder;

        public List<ProgramLogItem> ProgramLog = new();
        public IEnumerable<AudioMetadata> AudioFiles = new List<AudioMetadata>();
        public List<AudioMetadata> DraggedAudioFiles = new();
        public List<Folder> Folders = new();
        private Folder? selectedFolder = null;

        [Inject] private ApplicationDbContext DbContext { get; set; }
        [Inject] private AppStateService AppStateService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Log.Information("--- Studio - OnInitializedAsync() -- Initializing Studio Page");
            AppStateService.OnStationChanged += HandleStationChanged;
            await LoadTodaysLog();
            await LoadFolders();
            StateHasChanged();
        }

        private async void HandleStationChanged(object? sender, EventArgs e)
        {
            await LoadFolders();
            await LoadTodaysLog();
            StateHasChanged();
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
            if (AppStateService.station != null)
            {
                Folders = await DbContext.Folders.Where(f => f.StationId == AppStateService.station.StationId).ToListAsync();
            }
            else
            {
                Folders = new List<Folder>();
            }
        }

        private async Task LoadAudioFiles()
        {
            if (selectedFolder != null)
            {
                AudioFiles = await DbContext.AudioFiles.Where(af => af.Folder == selectedFolder.Name).ToListAsync();
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

            if (AppStateService.station != null)
            {
                ProgramLog = await DbContext.Log
                    .Where(li => li.StationId == AppStateService.station.StationId)
                    .OrderBy(li => li.LogID)
                    .ToListAsync();
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

            if (AppStateService.station != null)
            {
                bool hasLogEntries = await DbContext.Log.AnyAsync(li => li.StationId == AppStateService.station.StationId);

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
            if (selectedAudioFile != null)
            {
                // Shift existing items' LogID
                var itemsToShift = await DbContext.Log
                    .Where(li => li.LogID >= index)
                    .ToListAsync();

                foreach (var item in itemsToShift)
                {
                    item.LogID++;
                }

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
                    LogID = index
                };

                await DbContext.Log.AddAsync(newLogItem);
                await DbContext.SaveChangesAsync();

                await LoadTodaysLog();

                // Update the UI without reloading the entire log
                StateHasChanged();
                selectedAudioFile = null;
            }
        }

        public async Task DeleteSelectedLogItem(ProgramLogItem logItem)
        {
            ProgramLog.Remove(logItem);
            DbContext.Log.Remove(logItem);
            await DbContext.SaveChangesAsync();
            StateHasChanged();
            _delete = false;
        }

        public void Dispose()
        {
            AppStateService.OnStationChanged -= HandleStationChanged;
        }
    }
}
