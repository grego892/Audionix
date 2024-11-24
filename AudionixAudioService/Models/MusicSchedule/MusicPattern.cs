namespace AudionixAudioServer.Models.MusicSchedule
{
    public class MusicPattern
    {
        public Guid PatternId { get; set; } // Change this to be the primary key
        public string Name { get; set; } = string.Empty;
        public Guid StationId { get; set; }
        public List<PatternCategory> PatternCategories { get; set; } = new();
    }
}