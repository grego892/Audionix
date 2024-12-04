namespace SharedLibrary.Models.MusicSchedule 
{
    public class MusicGridItem
    {
        public int Id { get; set; }
        public string Hour { get; set; }
        public string Sunday { get; set; } = string.Empty;
        public string Monday { get; set; } = string.Empty;
        public string Tuesday { get; set; } = string.Empty;
        public string Wednesday { get; set; } = string.Empty;
        public string Thursday { get; set; } = string.Empty;
        public string Friday { get; set; } = string.Empty;
        public string Saturday { get; set; } = string.Empty;

        // Add properties to store PatternId
        public Guid? SundayPatternId { get; set; }
        public Guid? MondayPatternId { get; set; }
        public Guid? TuesdayPatternId { get; set; }
        public Guid? WednesdayPatternId { get; set; }
        public Guid? ThursdayPatternId { get; set; }
        public Guid? FridayPatternId { get; set; }
        public Guid? SaturdayPatternId { get; set; }

        public Guid StationId { get; set; }
    }
}
