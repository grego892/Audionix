namespace Audionix.Models.MusicSchedule
{
    public class Category
    {
        public int Id { get; set; }
        public string? CategoryName { get; set; }
        public Guid StationId { get; set; }

        // Foreign key to Template
        public int? TemplateId { get; set; }
        public MusicPattern? Template { get; set; }
    }
}
