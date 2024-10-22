using Audionix.Data.StationLog;
using Audionix.Models;
using Audionix.Models.MusicSchedule;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

namespace Audionix.Components.Pages.MusicSchedule
{
    public partial class Patterns
    {
        private string? selectedMusicPatternName;
        private string newMusicPatternName = string.Empty;
        private List<MusicPattern> filteredMusicPatternNames = new();
        private List<string> MusicPatternNames = new List<string>();
        private List<Categories> MusicPatternDataList = new List<Categories>();
        private List<Station> stations = new();
        private Guid? selectedStationId;

        protected override async Task OnInitializedAsync()
        {
            stations = await DbContext.Stations.ToListAsync();
            MusicPatternNames = await DbContext.MusicPatterns.Select(mp => mp.Name!).ToListAsync();

            selectedStationId = null;
        }

        private async Task OnStationChanged(Guid? stationId)
        {
            selectedStationId = stationId;
            await FilterPatterns();
        }

        private async Task FilterPatterns()
        {
            if (selectedStationId.HasValue)
            {
                filteredMusicPatternNames = await DbContext.MusicPatterns
                    .Where(c => c.StationId == selectedStationId.Value)
                    .ToListAsync();
            }
            else
            {
                filteredMusicPatternNames = new List<MusicPattern>();
            }
        }

        private async Task AddMusicPattern()
        {
            if (selectedStationId.HasValue)
            {
                var newMusicPattern = new MusicPattern { Name = newMusicPatternName, StationId = selectedStationId.Value };
                DbContext.MusicPatterns.Add(newMusicPattern);
                await DbContext.SaveChangesAsync();
                newMusicPatternName = string.Empty;

                await FilterPatterns();
                StateHasChanged();
            }
        }

        private async Task RemoveMusicPattern()
        {
            var musicPatternToRemove = await DbContext.MusicPatterns.FirstOrDefaultAsync(mp => mp.Name == selectedMusicPatternName);
            if (musicPatternToRemove != null)
            {
                DbContext.MusicPatterns.Remove(musicPatternToRemove);
                await DbContext.SaveChangesAsync();
                MusicPatternNames.Remove(selectedMusicPatternName!);
                selectedMusicPatternName = null;
                await FilterPatterns();
                StateHasChanged();
            }
        }

        private void OpenDialogRemoveMusicPattern()
        {
            var parameters = new DialogParameters();
            parameters.Add("OnConfirm", new Action(async () => await RemoveMusicPattern()));

            var options = new DialogOptions { CloseOnEscapeKey = true };
            DialogService.Show<TemplateDeleteDialog>("Are you sure you want to delete this pattern?", parameters, options);
        }

        private void SelectDefaultItem()
        {
            if (selectedMusicPatternName != null)
            {
                selectedMusicPatternName = MusicPatternNames.FirstOrDefault();
            }
        }
    }
}
