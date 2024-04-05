namespace Audionix.Models
{
    public class MusicPatternData
    {
        public int Id { get; set; }
        public string? Category { get; set; }
        public MusicPatternData DeepCopy()
        {
            MusicPatternData copy = new()
            {
                Id = Id,
                Category = Category
            };
            return copy;
        }
    }
}
