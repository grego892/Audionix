using System.ComponentModel.DataAnnotations;

namespace Audionix.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string? CategoryName { get; set; }
        public Guid StationId { get; set; }

        // Foreign key to Template
        public int? TemplateId { get; set; }
        public Template? Template { get; set; }
    }

    public class Template
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid StationId { get; set; }
        public List<Category> TemplateCategories { get; set; } = new List<Category>();
    }
    public class Grid
    {
        // 7 days a week, 24 hours a day
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
