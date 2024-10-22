namespace Audionix.Models.MusicSchedule
{
    public class MusicPattern
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid StationId { get; set; }
        public List<Category> PatternCategories { get; set; } = new List<Category>();
    }
}
