namespace SharedLibrary.Models
{
    public class AudioDevice
    {
        public int Id { get; set; } // Add this line to define the primary key
        public string? DeviceID { get; set; }
        public string? FriendlyName { get; set; }
    }
}
