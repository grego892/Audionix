namespace Audionix.Models
{
    public class ProgramLogItem
    {
        public int Id { get; set; }
        public int LogID { get; set; }
        public string? Status { get; set; }
        public string? Cue { get; set; }
        public string? Title { get; set; }
        public string? Artist { get; set; }
        public string? Scheduled { get; set; }
        public string? Actual { get; set; }
        public string? Name { get; set; }
        public string? Cart { get; set; }
        public string? Length { get; set; }
        public string? Segue { get; set; }
        public string? Category { get; set; }
        public string? From { get; set; }
        public string? Description { get; set; }
        public string? Passthrough { get; set; }
        public string? States { get; set; }
        public int? Device { get; set; }
        public int? sID { get; set; }
        public string? Estimated { get; set; }
        public double Progress { get; set; }

        // Foreign key to Station
        public Guid StationId { get; set; }
        public Station? Station { get; set; }
    }

    // Enums remain unchanged
    enum CategoryType
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

    enum CueType
    {
        Stop,
        AutoStart,
        TimeImmediate,
        TimeNext
    }

    enum FromType
    {
        CLOCKS,
        TRAFFIC
    }

    enum StatesType
    {
        isReady
    }
}
