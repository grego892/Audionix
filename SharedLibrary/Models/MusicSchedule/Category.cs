namespace SharedLibrary.Models.MusicSchedule
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int StationId { get; set; }
        public int? TemplateId { get; set; }
        public MusicPattern? Template { get; set; }
        public List<PatternCategory> PatternCategories { get; set; } = new List<PatternCategory>();
    }
}
