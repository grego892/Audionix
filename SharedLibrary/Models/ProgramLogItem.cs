using SharedLibrary.Models.MusicSchedule.Rules;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedLibrary.Models
{
    public class ProgramLogItem
    {
        public int LogOrderID { get; set; }
        public string? Cue { get; set; }
        public string? Title { get; set; }
        public string? Artist { get; set; }

        [Column(TypeName = "date")]
        public DateOnly Date { get; set; }

        [Column(TypeName = "time")]
        public TimeOnly TimeScheduled { get; set; }

        [Column(TypeName = "time")]
        public TimeOnly TimeEstimated { get; set; }

        [Column(TypeName = "time")]
        public TimeOnly TimePlayed { get; set; }

        public string? Name { get; set; }
        public Rotator? Rotator { get; set; }
        public TimeSpan Length { get; set; }
        public Int16 Intro { get; set; }
        public Int16 Segue { get; set; }
        public string? SongCategory { get; set; }
        //public EventType? EventType { get; set; }
        public string? From { get; set; }
        public string? Description { get; set; }
        public string? Passthrough { get; set; }
        public StatusType? Status { get; set; }
        public int? Device { get; set; }
        public double Progress { get; set; }

        // Foreign key to Station
        public int StationId { get; set; }
        public Station? Station { get; set; }
        public int? SoundCodeId { get; set; }
        public SoundCode? SoundCode { get; set; }
    }

    public enum CueType
    {
        Wait,
        AutoStart,
        TimeImmediate,
        TimeNext
    }

    public enum StatusType
    {
        notPlayed,
        isPlaying,
        hasPlayed
    }

    public class Rotator
    {
        [Key]
        public int RotatorID { get; set; }
        public string? RotatorTitle { get; set; }
        public string? RotatorArtist { get; set; }
        public string? Name { get; set; }
        public double Length { get; set; }
        public Int16 Intro { get; set; }
        public Int16 Segue { get; set; }
        public string? Description { get; set; }
        public int StationId { get; set; }
    }
}
