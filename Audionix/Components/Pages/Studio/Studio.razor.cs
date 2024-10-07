using Audionix.Data.StationLog;
using Microsoft.AspNetCore.Identity;
using MudBlazor;
using Serilog;
using Audionix.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components;
using MudBlazor.Utilities;

namespace Audionix.Components.Pages.Studio
{
    public partial class Studio
    {
        private bool _open = false;
        private bool _delete = false;
        private AudioMetadata? selectedAudioFile;
        private ProgramLogItem? selectedLogItem;

        public List<ProgramLogItem> ProgramLog = new();
        public IEnumerable<AudioMetadata> AudioFiles = new List<AudioMetadata>();
        public List<AudioMetadata> DraggedAudioFiles = new();

        [Inject]
        public required AudionixDbContext DbContext { get; set; }

        private async Task LoadTodaysLog()
        {
            Log.Information("--- Studio - LoadTodaysLog() -- Loading Today's Log");

            ProgramLog = await DbContext.Log.ToListAsync();
        }

        private async Task LoadAudioFiles()
        {
            Log.Information("--- Studio - LoadAudioFiles() -- Loading Audio Files");

            AudioFiles = await DbContext.AudioFiles.ToListAsync();
        }

        public async Task UpdateProgramLogItemAsync(int id, string newStatus, string newDescription)
        {
            var programLogItem = await DbContext.Log.FindAsync(id);

            if (programLogItem != null)
            {
                programLogItem.Status = newStatus;
                programLogItem.Description = newDescription;

                await DbContext.SaveChangesAsync();
            }
            else
            {
                Log.Error($"ProgramLogItem with ID {id} not found.");
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

        protected override async Task OnInitializedAsync()
        {
            Log.Information("--- Studio - OnInitializedAsync() -- Initializing Studio Page");
            await LoadTodaysLog();
            await LoadAudioFiles();
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
        }

        private bool IsAudioFileSelected => selectedAudioFile != null;


        private void SelectLogItem(ProgramLogItem logItem)
        {
            selectedLogItem = logItem;
        }

        public async Task AddSelectedAudioToLog(ProgramLogItem logItem, int index)
        {
            if (selectedAudioFile != null)
            {
                var newLogItem = new ProgramLogItem
                {
                    Name = selectedAudioFile.Title,
                    Description = selectedAudioFile.Artist,
                    Category = "AUDIO",
                    Progress = 0.0,
                    Scheduled = DateTime.Now.ToString("HH:mm:ss"),
                    Actual = DateTime.Now.ToString("HH:mm:ss"),
                    Status = "PLAYING",
                    StationId = logItem.StationId
                };

                // Insert the new log item at the specified index
                ProgramLog.Insert(index, newLogItem);

                await DbContext.Log.AddAsync(newLogItem);
                await DbContext.SaveChangesAsync();

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
