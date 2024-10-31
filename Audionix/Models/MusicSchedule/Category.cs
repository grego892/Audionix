namespace Audionix.Models.MusicSchedule
{
    public class Category
    {
        public Guid CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public Guid StationId { get; set; }
        public int? TemplateId { get; set; }
        public MusicPattern? Template { get; set; }
        public List<PatternCategory> PatternCategories { get; set; } = new List<PatternCategory>();
    }
}
