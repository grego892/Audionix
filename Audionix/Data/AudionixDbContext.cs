using Audionix.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static Audionix.Components.Pages.FileManager.FileManager;
using static System.Collections.Specialized.BitVector32;

namespace Audionix.Models
{
    public class AudionixDbContext(DbContextOptions<AudionixDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Station> Stations { get; set; }
        public DbSet<MusicPattern> MusicPatterns { get; set; }
        public DbSet<AudioMetadata> AudioFiles { get; set; }
        public DbSet<Folder> Folders { get; set; }


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
        }

    }
}
