using Microsoft.AspNetCore.Components;

namespace Audionix.Models
{
    public class AppSettings
    {
        public string? ConfigFolder { get; set; }
        public string? DataPath { get; set; }
        public string? DatabasePath { get; set; }
        public bool IsDatapathSetup { get; set; }
        public string? LoggingPath { get; set; }
    }
}
