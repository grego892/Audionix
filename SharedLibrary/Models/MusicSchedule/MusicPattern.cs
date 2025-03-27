namespace SharedLibrary.Models.MusicSchedule
{
    public class MusicPattern
    {
        public int PatternId { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid StationId { get; set; }
        public List<PatternCategory> PatternCategories { get; set; } = new();
    }
}