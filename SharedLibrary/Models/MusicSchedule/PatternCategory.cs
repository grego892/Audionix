using SharedLibrary.Models.MusicSchedule.Rules;

namespace SharedLibrary.Models.MusicSchedule
{
    //public class PatternCategory
    //{
    //    public int Id { get; set; }
    //    public int MusicPatternSortOrder { get; set; }
    //    public int MusicPatternId { get; set; }
    //    public int StationId { get; set; }
    //    public MusicPattern MusicPattern { get; set; } = null!;
    //    public SongCategory SongCategory { get; set; } = null!;
    //}

    public class PatternCategory
    {
        public int Id { get; set; }
        public int MusicPatternSortOrder { get; set; }
        public int MusicPatternId { get; set; }
        public MusicPattern MusicPattern { get; set; } = null!;
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;
        public string CategoryName { get; set; } = string.Empty;
        public int StationId { get; set; }
    }
}
