using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Models.MusicSchedule.Rules
{
    public class SoundCode
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public int StationId { get; set; }
    }
}
