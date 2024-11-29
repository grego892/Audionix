using System.ComponentModel.DataAnnotations.Schema;

namespace AudionixAudioServer.Models
{
    public class ProgramLogItem
    {
        public int Id { get; set; }
        public int LogOrderID { get; set; }
        public string? Status { get; set; }
        public string? Cue { get; set; }
        public string? Title { get; set; }
        public string? Artist { get; set; }

        [Column(TypeName = "date")]
        public DateOnly? Date { get; set; }

        [Column(TypeName = "time")]
        public TimeOnly TimeScheduled { get; set; }

        [Column(TypeName = "time")]
        public TimeOnly TimeEstimated { get; set; }

        [Column(TypeName = "time")]
        public TimeOnly TimePlayed { get; set; }

        public string? Name { get; set; }
        public string? Cart { get; set; }
        public string? Length { get; set; }
        public string? Intro { get; set; }
        public string? Segue { get; set; }
        public string? Category { get; set; }
        public string? From { get; set; }
        public string? Description { get; set; }
        public string? Passthrough { get; set; }
        public StatesType? States { get; set; }
        public int? Device { get; set; }
        public int? sID { get; set; }
        public double Progress { get; set; }

        // Foreign key to Station
        public Guid StationId { get; set; }
        public Station? Station { get; set; }
    }

    public enum CategoryType
    {
        SONG,
        SPOT,
        AUDIO,
        MACRO,
        VOICETRACK,
        ERROR,
        DONE,
        PLAYING,
        COMMENT,
        CART
    }

    public enum CueType
    {
        Stop,
        AutoStart,
        TimeImmediate,
        TimeNext
    }

    public enum FromType
    {
        CLOCKS,
        TRAFFIC
    }

    public enum StatesType
    {
        notPlayed,
        isPlaying,
        hasPlayed
    }
}
