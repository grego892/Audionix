using Audionix.Data.StationLog;
using Audionix.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Audionix.Models
{
    public class AudionixDbContext : IdentityDbContext<ApplicationUser>
    {
        public AudionixDbContext(DbContextOptions<AudionixDbContext> options) : base(options) { }

        public DbSet<Station> Stations { get; set; }
        public DbSet<MusicPattern> MusicPatterns { get; set; }
        public DbSet<AudioMetadata> AudioFiles { get; set; }
        public DbSet<Folder> Folders { get; set; }
        public DbSet<ProgramLogItem> Log { get; set; }
        public DbSet<MusicCategory> MusicCategories { get; set; }
    }
}
