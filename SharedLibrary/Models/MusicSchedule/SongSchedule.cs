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
        public Category SongCategory { get; set; }
        public Tempo SongTempo { get; set; }
        public EnergyLevel SongEnergyLevel { get; set; }
        public int PlayCount { get; set; }
        public int LastPlayed { get; set; }

        public enum Category
        {
            Current,
            Recurrent,
            Gold,
            Power,
            Specialty,
            Holiday,
            Custom,
            None
        }
        public enum Tempo
        {
            Slow,
            Medium,
            Fast
        }
        public enum EnergyLevel
        {
            Low,
            Medium,
            High
        }

    }
}
