using System.ComponentModel.DataAnnotations.Schema;

namespace SharedLibrary.Models
{
    public class Station
    {
        public Guid StationId { get; set; } = Guid.Empty;
        public int StationSortOrder { get; set; }
        public string? CallLetters { get; set; }
        public string? Slogan { get; set; }
        public string AudioDeviceId { get; set; }
        public int CurrentPlayingId { get; set; } = 1;
        [Column(TypeName = "date")]
        public DateOnly? CurrentPlayingDate { get; set; }
        public int NextPlayId { get; set; } = 1;
        [Column(TypeName = "date")]
        public DateOnly? NextPlayDate { get; set; }
        public ICollection<AudioMetadata>? AudioFiles { get; set; }
        public ICollection<Folder>? Folders { get; set; }
        public ICollection<ProgramLogItem>? ProgramLogItems { get; set; } // Navigation property
        public Station()
        {
            StationId = Guid.NewGuid(); // Automatically generate a new GUID
        }

        public Station DeepCopy()
        {
            return new Station
            {
                StationId = this.StationId,
                StationSortOrder = this.StationSortOrder,
                CallLetters = this.CallLetters,
                Slogan = this.Slogan,
                AudioDeviceId = this.AudioDeviceId,
                AudioFiles = this.AudioFiles?.Select(af => new AudioMetadata
                {
                    StationId = af.StationId,
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
                    Station = af.Station,
                    Folder = af.Folder
                }).ToList(),
                Folders = this.Folders?.Select(f => new Folder
                {
                    StationId = f.StationId,
                    Id = f.Id,
                    Name = f.Name,
                    Station = f.Station
                }).ToList(),
                ProgramLogItems = this.ProgramLogItems?.Select(pl => new ProgramLogItem
                {
                    StationId = pl.StationId,
                    Status = pl.Status,
                    Cue = pl.Cue,
                    Name = pl.Name,
                    Rotator = pl.Rotator,
                    Length = pl.Length,
                    Segue = pl.Segue,
                    SongCategory = pl.SongCategory,
                    From = pl.From,
                    Description = pl.Description,
                    Passthrough = pl.Passthrough,
                    Device = pl.Device,
                    TimeEstimated = pl.TimeEstimated,
                    Progress = pl.Progress,
                    Station = pl.Station
                }).ToList()
            };
        }
    }
}
