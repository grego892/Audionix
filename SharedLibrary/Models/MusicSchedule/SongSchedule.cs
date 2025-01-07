using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Models.MusicSchedule
{
    public class SongSchedule
    {
        public int Id { get; set; }
        public int AudioMetadataId { get; set; }
        public AudioMetadata? AudioMetadata { get; set; }

        //Play history
        public int PlayCount { get; set; }
        public int LastPlayed { get; set; }
        public List<string> Category { get; set; } = new List<string>
        {
            "Current",
            "Recurrent",
            "Gold",
            "Power",
            "Specialty",
            "Holiday",
            "Liner",
            "Custom",
            "None"
        };

        public string? SoundCode { get; set; }
        public List<string> Tempo { get; set; } = new List<string>
        {
            "Slow",
            "Medium",
            "Fast"
        };
        public List<string> EnergyLevel { get; set; } = new List<string>
        {
            "Low",
            "Medium",
            "High"
        };
        public string? Daypart { get; set; }
        public string? Level { get; set; }
        public string? Soundwave { get; set; }
    }
}
