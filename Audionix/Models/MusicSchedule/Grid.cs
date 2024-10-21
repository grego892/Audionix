using System.ComponentModel.DataAnnotations.Schema;

namespace Audionix.Models.MusicSchedule
{
    public class Grid
    {
        public int Id { get; set; }

        [NotMapped]
        public Template[,] Schedule { get; set; } = new Template[7, 24];

        // Optional: Method to assign a template to a specific day and hour
        public void AssignTemplate(int day, int hour, Template template)
        {
            if (day < 0 || day > 6)
                throw new ArgumentOutOfRangeException(nameof(day), "Day must be between 0 (Sunday) and 6 (Saturday).");
            if (hour < 0 || hour > 23)
                throw new ArgumentOutOfRangeException(nameof(hour), "Hour must be between 0 and 23.");

            Schedule[day, hour] = template;
        }
    }
}
