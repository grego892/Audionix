using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Models.MusicSchedule.Rules
{
    public class Category
    {
        public Guid CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public Guid StationId { get; set; }
        public int? TemplateId { get; set; }
        public MusicPattern? Template { get; set; }
        public List<PatternCategory> PatternCategories { get; set; } = new List<PatternCategory>();
    }
}


//public int Id { get; set; }
//public string? Name { get; set; }
//public Guid StationId { get; set; }