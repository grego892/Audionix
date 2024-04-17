namespace Audionix.Models
{
    public class Folder
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int StationId { get; set; } // Foreign key property
        public Station? Station { get; set; } // Navigation property
    }
}
