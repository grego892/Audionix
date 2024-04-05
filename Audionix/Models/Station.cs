namespace Audionix.Models
{
    public class Station
    {
        public int Id { get; set; }
        public string? CallLetters { get; set; }
        public string? Slogan { get; set; }
        public Station DeepCopy()
        {
            Station copy = new()
            {
                Id = Id,
                CallLetters = CallLetters,
                Slogan = Slogan
            };
            return copy;
        }
    }

}
