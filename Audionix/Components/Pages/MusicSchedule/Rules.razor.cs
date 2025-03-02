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
            if (!string.IsNullOrEmpty(newCategory))
            {
                songSchedule.Category.Add(newCategory);
                newCategory = string.Empty;
            }
        }

        public void RemoveCategory(string? category)
        {
            if (category != null)
            {
                songSchedule.Category.Remove(category);
            }
        }

        public void AddSoundCode()
        {
            if (!string.IsNullOrEmpty(newSoundCode))
            {
                songSchedule.SoundCode.Add(newSoundCode);
                newSoundCode = string.Empty;
            }
        }

        public void RemoveSoundCode(string soundCode)
        {
            songSchedule.SoundCode.Remove(soundCode);
        }

        public void AddEnergyLevel()
        {
            if (!string.IsNullOrEmpty(newEnergyLevel))
            {
                songSchedule.EnergyLevel.Add(newEnergyLevel);
                newEnergyLevel = string.Empty;
            }
        }

        public void RemoveEnergyLevel(string? energyLevel)
        {
            if (energyLevel != null)
            {
                songSchedule.EnergyLevel.Remove(energyLevel);
            }
        }

        private void RemoveCategoryHandler(string category)
        {
            RemoveCategory(category);
        }

        private void RemoveSoundCodeHandler(string soundCode)
        {
            RemoveSoundCode(soundCode);
        }

        private void RemoveEnergyLevelHandler(string energyLevel)
        {
            RemoveEnergyLevel(energyLevel);
        }
    }
}
