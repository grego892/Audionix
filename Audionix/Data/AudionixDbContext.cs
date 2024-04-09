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
        public DbSet<MusicPatternData> MusicPatternsData { get; set; }
        public DbSet<AudioMetadata> AudioMetadatas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AudioMetadata>()
                .HasOne(am => am.Station)
                .WithMany(s => s.AudioMetadatas)
                .HasForeignKey(am => am.StationId);
        }
    }
}
