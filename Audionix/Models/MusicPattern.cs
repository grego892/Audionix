namespace Audionix.Models
{
    public class MusicPattern
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public ICollection<MusicPatternData>? MusicPatternData { get; set; }
        public MusicPattern DeepCopy()
        {
            MusicPattern copy = new()
            {
                Id = Id,
                Name = Name,
            };
            return copy;
        }
    }
}
