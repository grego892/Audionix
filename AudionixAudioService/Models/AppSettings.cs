namespace AudionixAudioServer.Models
{
    public class AppSettings
    {
        public int Id { get; set; } = 1;
        public string? DataPath { get; set; } = "C:\\Program Files\\Audionix\\AudionixAudio";
        public bool IsDatapathSetup { get; set; } = false;
    }
}
