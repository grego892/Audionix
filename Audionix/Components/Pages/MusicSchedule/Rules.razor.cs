namespace Audionix.Components.Pages.MusicSchedule
{
    public partial class Rules
    {
        private string newCategory;
        private string newSoundCode;
        private string newEnergyLevel;
        private SongSchedule songSchedule = new SongSchedule();

        protected override async Task OnInitializedAsync()
        {
            async Task LoadTables();
        }

        private asymc Task LoadTables()
        {
            
        }

        private void AddCategory()
        {
            if (!string.IsNullOrWhiteSpace(newCategory))
            {
                songSchedule.Category.Add(newCategory);
                newCategory = string.Empty;
            }
        }

        private void RemoveCategoryHandler(string category)
        {
            songSchedule.Category.Remove(category);
        }

        private void AddSoundCode()
        {
            if (!string.IsNullOrWhiteSpace(newSoundCode))
            {
                songSchedule.SoundCode.Add(newSoundCode);
                newSoundCode = string.Empty;
            }
        }

        private void RemoveSoundCodeHandler(string soundCode)
        {
            songSchedule.SoundCode.Remove(soundCode);
        }

        private void AddEnergyLevel()
        {
            if (!string.IsNullOrWhiteSpace(newEnergyLevel))
            {
                songSchedule.EnergyLevel.Add(newEnergyLevel);
                newEnergyLevel = string.Empty;
            }
        }

        private void RemoveEnergyLevelHandler(string energyLevel)
        {
            songSchedule.EnergyLevel.Remove(energyLevel);
        }

        private class SongSchedule
        {
            public List<string> Category { get; set; } = new List<string>();
            public List<string> SoundCode { get; set; } = new List<string>();
            public List<string> EnergyLevel { get; set; } = new List<string>();
        }
    }
}
