using SharedLibrary.Models.MusicSchedule;
using SharedLibrary.Models.MusicSchedule.Rules;

namespace SharedLibrary.Models
{
    public class AudioMetadata
    {
        public int Id { get; set; }
        public string Filename { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public Int16 Intro { get; set; }
        public Int16 Segue { get; set; }
        public int StartDate { get; set; }
        public int EndDate { get; set; }
        public bool ProtectNextIntro { get; set; }
        public double IntroSeconds { get; set; }
        public double SegueSeconds { get; set; }
        public TimeSpan Duration { get; set; }
        public int StationId { get; set; }
        public Station? Station { get; set; }
        public string? Folder { get; set; }
        //public EventType EventType { get; set; }
        public int? CategoryId { get; set; }
        public SongCategory? SongCategory { get; set; }
        public int? SoundCodeId { get; set; }
        public SoundCode? SoundCode { get; set; }
        public int? EnergyLevelId { get; set; }
        public EnergyLevel? EnergyLevel { get; set; }
    }

    //public enum EventType
    //{
    //    song,
    //    liner,
    //    promo,
    //    spot,
    //    macro
    //}
}
