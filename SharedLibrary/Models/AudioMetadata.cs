using SharedLibrary.Models.MusicSchedule.Rules;

namespace SharedLibrary.Models
{
    public class AudioMetadata
    {
        public int Id { get; set; }
        public string Filename { get; set; } = "";
        public string Title { get; set; } = "";
        public string Artist { get; set; } = "";
        public Int16 Intro { get; set; } = 0;
        public Int16 Segue { get; set; } = 0;
        public int StartDate { get; set; } = 0;
        public int EndDate { get; set; }
        public bool NoFade { get; set; }
        public bool ProtectNextIntro { get; set; }
        public double IntroSeconds { get; set; } = 0;
        public double SegueSeconds { get; set; } = 0;
        public TimeSpan Duration { get; set; }

        public Guid StationId { get; set; }
        public Station? Station { get; set; }
        public string? Folder { get; set; }
        public string? SongCategory { get; set; }
        public EventType EventType { get; set; }
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }
        public SoundCode? SoundCode;
        public EnergyLevel? EnergyLevel;
    }
    public enum EventType
    {
        song,
        liner,
        promo,
        spot,
        macro
    }
}
