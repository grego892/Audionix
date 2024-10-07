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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AudioMetadata>()
                .HasOne(am => am.Station)
                .WithMany(s => s.AudioFiles)
                .HasForeignKey(am => am.StationId);

            modelBuilder.Entity<Folder>()
                .HasOne(f => f.Station)
                .WithMany(s => s.Folders)
                .HasForeignKey(f => f.StationId);

            modelBuilder.Entity<ProgramLogItem>()
                .HasOne(pl => pl.Station)
                .WithMany(s => s.ProgramLogItems)
                .HasForeignKey(pl => pl.StationId);

            // Seed data for ProgramLogItem
            modelBuilder.Entity<ProgramLogItem>().HasData(
                new ProgramLogItem
                {
                    Id = 1,
                    Status = "Initialized",
                    Cue = "AutoStart",
                    Scheduled = DateTime.Now.ToString("HH:mm:ss"),
                    Actual = DateTime.Now.ToString("HH:mm:ss"),
                    Name = "Default Log Entry",
                    Cart = "Default Cart",
                    Length = "00:00:30",
                    Segue = "00:00:05",
                    Category = "COMMENT",
                    From = "SYSTEM",
                    Description = "This is a default log entry.",
                    Passthrough = "None",
                    States = "isReady",
                    Device = 1,
                    sID = 1,
                    Estimated = DateTime.Now.AddMinutes(1).ToString("HH:mm:ss"),
                    Progress = 0.0,
                    StationId = 1 // Assuming a default station with ID 1 exists
                }
            );
        }
    }
}
