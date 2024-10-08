using Audionix.Data.StationLog;
using Microsoft.AspNetCore.Identity;
using MudBlazor;
using Serilog;
using Audionix.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components;
using MudBlazor.Utilities;
using static ATL.Logging.Log;

namespace Audionix.Components.Pages.Studio
{
    public partial class Studio
    {
        private bool _open = false;
        private bool _delete = false;
        private AudioMetadata? selectedAudioFile;
        private ProgramLogItem? selectedLogItem;
        private string? _selectedAudioFolder;

        public List<ProgramLogItem> ProgramLog = new();
        public IEnumerable<AudioMetadata> AudioFiles = new List<AudioMetadata>();
        public List<AudioMetadata> DraggedAudioFiles = new();
        public List<Station> Stations = new();
        public List<Folder> Folders = new();

        private Station? selectedStation;
        private Folder? selectedFolder;

        [Inject]
        public required AudionixDbContext DbContext { get; set; }

        private async Task LoadStations()
        {
            Stations = await DbContext.Stations.ToListAsync();
        }

        private async Task LoadFolders()
        {
            if (selectedStation != null)
            {
                Folders = await DbContext.Folders.Where(f => f.StationId == selectedStation.Id).ToListAsync();
            }
        }

        private async Task LoadAudioFiles()
        {
            if (selectedFolder != null)
            {
                AudioFiles = await DbContext.AudioFiles.Where(af => af.Folder == selectedFolder.Name).ToListAsync();
            }
        }

        private async Task OnStationChanged(Station station)
        {
            selectedStation = station;
            selectedFolder = null;
            AudioFiles = new List<AudioMetadata>();
            await LoadFolders();
        }

        private async Task OnFolderChanged(Folder folder)
        {
            selectedFolder = folder;
            await LoadAudioFiles();
        }

        private async Task LoadTodaysLog()
        {
            Log.Information("--- Studio - LoadTodaysLog() -- Loading Today's Log");

            ProgramLog = await DbContext.Log
                .OrderBy(li => li.LogID)
                .ToListAsync();
        }


        //public async Task UpdateProgramLogItemAsync(int id, string newStatus, string newDescription)
        //{
        //    var programLogItem = await DbContext.Log.FindAsync(id);

        //    if (programLogItem != null)
        //    {
        //        programLogItem.Status = newStatus;
        //        programLogItem.Description = newDescription;

        //        await DbContext.SaveChangesAsync();
        //    }
        //    else
        //    {
        //        Log.Error($"ProgramLogItem with ID {id} not found.");
        //    }
        //}

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

        protected override async Task OnInitializedAsync()
        {
            Log.Information("--- Studio - OnInitializedAsync() -- Initializing Studio Page");
            await LoadStations();
            await LoadTodaysLog();
            StateHasChanged();
        }

        private void ToggleDrawer()
        {
            _open = !_open;
        }
        private void ToggleDelete()
        {
            _delete = !_delete;
        }

        private void SelectAudioFile(AudioMetadata audioFile)
        {
            selectedAudioFile = audioFile;
            _open = false;

            if (DbContext.Log.Count() == 0)
            {
                if (selectedLogItem == null)
                {
                    selectedLogItem = new ProgramLogItem
                    {
                        // Initialize with default values
                        StationId = 1 // Assuming a default station with ID 1 exists
                    };
                }

                AddSelectedAudioToLog(1, selectedLogItem);
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
                    Scheduled = DateTime.Now.ToString("HH:mm:ss"),
                    Actual = DateTime.Now.ToString("HH:mm:ss"),
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
    }
}
