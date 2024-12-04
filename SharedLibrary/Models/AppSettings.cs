namespace SharedLibrary.Models
{
    public class AppSettings
    {
        public int Id { get; set; }
        public string? DataPath { get; set; }
        public bool IsDatapathSetup { get; set; }
    }
}
