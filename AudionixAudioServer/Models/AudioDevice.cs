namespace AudionixAudioServer.Models
{
    public class AudioDevice
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? DeviceID { get; set; }
        public string? FriendlyName { get; set; }
    }
}
