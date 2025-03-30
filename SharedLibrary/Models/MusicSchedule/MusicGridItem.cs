namespace SharedLibrary.Models.MusicSchedule 
{
    public class MusicGridItem
    {
        public int Id { get; set; }
        public string? Hour { get; set; }
        public string Sunday { get; set; } = string.Empty;
        public string Monday { get; set; } = string.Empty;
        public string Tuesday { get; set; } = string.Empty;
        public string Wednesday { get; set; } = string.Empty;
        public string Thursday { get; set; } = string.Empty;
        public string Friday { get; set; } = string.Empty;
        public string Saturday { get; set; } = string.Empty;

        // Add properties to store PatternId
        public int? SundayPatternId { get; set; }
        public int? MondayPatternId { get; set; }
        public int? TuesdayPatternId { get; set; }
        public int? WednesdayPatternId { get; set; }
        public int? ThursdayPatternId { get; set; }
        public int? FridayPatternId { get; set; }
        public int? SaturdayPatternId { get; set; }

        public int StationId { get; set; }
    }
}
