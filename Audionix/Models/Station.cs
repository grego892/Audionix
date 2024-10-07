using Audionix.Data.StationLog;

namespace Audionix.Models
{
    public class Station
    {
        public int Id { get; set; }
        public string? CallLetters { get; set; }
        public string? Slogan { get; set; }
        public ICollection<AudioMetadata>? AudioFiles { get; set; }
        public ICollection<Folder>? Folders { get; set; }
        public ICollection<ProgramLogItem>? ProgramLogItems { get; set; } // Navigation property

        public Station DeepCopy()
        {
            return new Station
            {
                Id = this.Id,
                CallLetters = this.CallLetters,
                Slogan = this.Slogan,
                AudioFiles = this.AudioFiles?.Select(af => new AudioMetadata
                {
                    Id = af.Id,
                    Filename = af.Filename,
                    Title = af.Title,
                    Artist = af.Artist,
                    Intro = af.Intro,
                    Segue = af.Segue,
                    StartDate = af.StartDate,
                    EndDate = af.EndDate,
                    NoFade = af.NoFade,
                    ProtectNextIntro = af.ProtectNextIntro,
                    IntroSeconds = af.IntroSeconds,
                    SegueSeconds = af.SegueSeconds,
                    Duration = af.Duration,
                    StationId = af.StationId,
                    Station = af.Station,
                    Folder = af.Folder
                }).ToList(),
                Folders = this.Folders?.Select(f => new Folder
                {
                    Id = f.Id,
                    Name = f.Name,
                    StationId = f.StationId,
                    Station = f.Station
                }).ToList(),
                ProgramLogItems = this.ProgramLogItems?.Select(pl => new ProgramLogItem
                {
                    Id = pl.Id,
                    Status = pl.Status,
                    Cue = pl.Cue,
                    Scheduled = pl.Scheduled,
                    Actual = pl.Actual,
                    Name = pl.Name,
                    Cart = pl.Cart,
                    Length = pl.Length,
                    Segue = pl.Segue,
                    Category = pl.Category,
                    From = pl.From,
                    Description = pl.Description,
                    Passthrough = pl.Passthrough,
                    States = pl.States,
                    Device = pl.Device,
                    sID = pl.sID,
                    Estimated = pl.Estimated,
                    Progress = pl.Progress,
                    StationId = pl.StationId,
                    Station = pl.Station
                }).ToList()
            };
        }
    }
}
