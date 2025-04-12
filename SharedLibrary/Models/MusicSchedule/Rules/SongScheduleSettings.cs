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
        public bool BreakArtistSeperation { get; set; }
        public bool BreakMaxSoundCodeSeperation { get; set; }
        public bool BreakMaxEnergySeperation { get; set; }
    }
}
