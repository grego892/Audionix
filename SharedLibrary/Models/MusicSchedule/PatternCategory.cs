namespace SharedLibrary.Models.MusicSchedule
{
    public class PatternCategory
    {
        public int Id { get; set; }
        public int MusicPatternSortOrder { get; set; }
        public int MusicPatternId { get; set; }
        public int StationId { get; set; }
        public MusicPattern MusicPattern { get; set; } = null!;
        public SongCategory SongCategory { get; set; } = null!;
    }
}
