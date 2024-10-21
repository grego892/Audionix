namespace Audionix.Models.MusicSchedule
{
    public class Template
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid StationId { get; set; }
        public List<Category> TemplateCategories { get; set; } = new List<Category>();
    }
}
