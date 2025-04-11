using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Models.MusicSchedule.Rules
{
    public class SongScheduleSettings
    {
        public int Id { get; set; }
        public int ArtistSeperation { get; set; }
        public int TitleSeperation { get; set; }
        public int MaxSoundcodeSeperation { get; set; }
        public int MaxEnergySeperation { get; set; }
    }
}
