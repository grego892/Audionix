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
    }
}
