namespace SharedLibrary.Models.MusicSchedule
{
    public class PatternCategory
    {
        public int Id { get; set; }
        public int MusicPatternSortOrder { get; set; }
        public int MusicPatternId { get; set; }
        public MusicPattern MusicPattern { get; set; } = null!;
        public int SongCategoryId { get; set; }
        public SongCategory SongCategory { get; set; } = null!;
        public string SongCategoryName { get; set; } = string.Empty;
        public Guid StationId { get; set; }
    }
}
