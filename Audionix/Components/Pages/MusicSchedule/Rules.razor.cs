using Microsoft.AspNetCore.Components;
using MudBlazor;
using SharedLibrary.Models.MusicSchedule;
using SharedLibrary.Repositories;
using System.Threading.Tasks;

namespace Audionix.Components.Pages.MusicSchedule
{
    public partial class Rules
    {
        private string? newCategory;
        private string? newSoundCode;
        private string? newTempo;
        private string? newEnergyLevel;
        private string? newDaypart;
        private string? newLevel;
        private string? newSoundwave;
        private SongSchedule songSchedule = new();

        public void AddCategory()
        {

        }
        public void RemoveCategory(string? category) { }
        public void AddSoundCode() { }
        public void RemoveSoundCode(char soundCode) { }
        public void NewEvergyLevel(SongSchedule songSchedule) { }
        public void RemoveEnergyLevel(string? energyLevel) { }
        public void AddEnergyLevelk(SongSchedule songSchedule) { }
        public void OnRowEnter(SongSchedule songSchedule) { }
        public void AddEnergyLevel() { }
    }
}
