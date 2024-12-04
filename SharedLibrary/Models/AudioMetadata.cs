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
        public double Duration { get; set; } = 0;

        public Guid StationId { get; set; }
        public Station? Station { get; set; }
        public string? Folder { get; set; }
        public string? Category { get; set; }
        public AudioType AudioType { get; set; }
    }
    public enum AudioType
    {
        song,
        liner,
        promo,
        spot,
        macro
    }
}
