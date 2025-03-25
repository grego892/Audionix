using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Models.MusicSchedule.Rules
{
    public class EnergyLevel
    {
        public int Id { get; set; }
        public string? Level { get; set; }
        public Guid StationId { get; set; }
    }
}
