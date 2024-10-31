using Audionix.Data;
using Audionix.Models.MusicSchedule;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Audionix.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Station> Stations { get; set; }
        public DbSet<AudioMetadata> AudioFiles { get; set; }
        public DbSet<Folder> Folders { get; set; }
        public DbSet<ProgramLogItem> Log { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<MusicPattern> MusicPatterns { get; set; }
        public DbSet<Grid> Grids { get; set; }
        public DbSet<PatternCategory> PatternCategories { get; set; }
        public DbSet<MusicGridItem> MusicGridItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MusicPattern>()
                .HasKey(mp => mp.PatternId); // Set PatternId as the primary key

            modelBuilder.Entity<PatternCategory>()
                .HasKey(pc => pc.Id); // Set Id as the primary key

            modelBuilder.Entity<PatternCategory>()
                .HasOne(pc => pc.MusicPattern)
                .WithMany(mp => mp.PatternCategories)
                .HasForeignKey(pc => pc.MusicPatternId);

            modelBuilder.Entity<PatternCategory>()
                .HasOne(pc => pc.Category)
                .WithMany(c => c.PatternCategories)
                .HasForeignKey(pc => pc.CategoryId);

            // Configure relationships if necessary
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

            // Configure Grid entity if necessary
            modelBuilder.Entity<Grid>()
                .HasKey(g => g.Id); // Assuming Grid has an Id property
        }
    }
}
