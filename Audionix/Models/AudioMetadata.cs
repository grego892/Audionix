namespace Audionix.Models
{
    public class AudioMetadata
    {
        public string Filename { get; set; } = "";
        public string Title { get; set; } = "";
        public string Artist { get; set; } = "";
        public int Intro { get; set; } = 0;
        public int Segue { get; set; } = 0;
        public int StartDate { get; set; } = 0;
        public int EndDate { get; set; }
        public bool NoFade { get; set; }
        public bool ProtectNextIntro { get; set; }
        public double IntroSeconds { get; set; } = 0;
        public double SegueSeconds { get; set; } = 0;
        public double Duration { get; set; } = 0;
    }
}
