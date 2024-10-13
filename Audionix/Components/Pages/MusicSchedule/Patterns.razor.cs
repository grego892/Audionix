using Audionix.Models;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Audionix.Components.Pages.MusicSchedule
{
    public partial class Patterns
    {
        private string? selectedMusicPatternName;
        private string newMusicPatternName = string.Empty;
        private List<string> MusicPatternNames = new List<string>();
        private List<MusicPatternData> MusicPatternDataList = new List<MusicPatternData>();

        protected override async Task OnInitializedAsync()
        {
            MusicPatternNames = await DbContext.MusicPatterns.Select(mp => mp.Name!).ToListAsync();
            SelectDefaultItem();
            if (selectedMusicPatternName != null)
            {
                var selectedMusicPattern = await DbContext.MusicPatterns.Include(mp => mp.MusicPatternData).FirstOrDefaultAsync(mp => mp.Name == selectedMusicPatternName);
                if (selectedMusicPattern != null)
                {
                    MusicPatternDataList = selectedMusicPattern.MusicPatternData.ToList();
                }
            }
        }

        private async Task AddMusicPattern()
        {
            var newMusicPattern = new MusicPattern { Name = newMusicPatternName };
            DbContext.MusicPatterns.Add(newMusicPattern);
            await DbContext.SaveChangesAsync();
            MusicPatternNames.Add(newMusicPatternName);
            newMusicPatternName = string.Empty;
            SelectDefaultItem();
            StateHasChanged();
        }

        private async Task RemoveMusicPattern()
        {
            var musicPatternToRemove = await DbContext.MusicPatterns.FirstOrDefaultAsync(mp => mp.Name == selectedMusicPatternName);
            if (musicPatternToRemove != null)
            {
                DbContext.MusicPatterns.Remove(musicPatternToRemove);
                await DbContext.SaveChangesAsync();
                MusicPatternNames.Remove(selectedMusicPatternName!);
                SelectDefaultItem();
            }
            StateHasChanged();
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
