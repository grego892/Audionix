namespace SharedLibrary.Models.MusicSchedule
{
    public class MusicPattern
    {
        public int PatternId { get; set; } // Change this to be the primary key
        public string Name { get; set; } = string.Empty;
        public int StationId { get; set; }
        public List<PatternCategory> PatternCategories { get; set; } = new();
    }
}