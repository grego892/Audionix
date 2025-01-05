namespace SharedLibrary.Models.MusicSchedule
{
    public class SongCategory
    {
        public Guid SongCategoryId { get; set; }
        public string? SongCategoryName { get; set; }
        public Guid StationId { get; set; }
        public int? TemplateId { get; set; }
        public MusicPattern? Template { get; set; }
        public List<PatternCategory> PatternCategories { get; set; } = new List<PatternCategory>();
    }
}
