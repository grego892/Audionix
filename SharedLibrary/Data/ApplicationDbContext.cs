using SharedLibrary.Models.MusicSchedule;
using SharedLibrary.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using SharedLibrary.Data;
using SharedLibrary.Models.MusicSchedule.Rules;

namespace SharedLibrary.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            Stations = Set<Station>();
            AudioFiles = Set<AudioMetadata>();
            Folders = Set<Folder>();
            Log = Set<ProgramLogItem>();
            SongCategories = Set<SongCategory>();
            MusicPatterns = Set<MusicPattern>();
            PatternCategories = Set<PatternCategory>();
            MusicGridItems = Set<MusicGridItem>();
            AppSettings = Set<AppSettings>();
        }

        public DbSet<AppSettings> AppSettings { get; set; }
        public DbSet<Station> Stations { get; set; }
        public DbSet<AudioMetadata> AudioFiles { get; set; }
        public DbSet<Folder> Folders { get; set; }
        public DbSet<ProgramLogItem> Log { get; set; }
        public DbSet<SongCategory> SongCategories { get; set; }
        public DbSet<MusicPattern> MusicPatterns { get; set; }
        public DbSet<PatternCategory> PatternCategories { get; set; }
        public DbSet<MusicGridItem> MusicGridItems { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<SoundCode> SoundCodes { get; set; }
        public DbSet<EnergyLevel> EnergyLevels { get; set; }
        public DbSet<SongScheduleSettings> SongScheduleSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MusicPattern>()
                .HasKey(mp => mp.PatternId); // Set PatternId as the primary key

            modelBuilder.Entity<PatternCategory>()
                .HasKey(pc => pc.Id); // Set Id as the primary key

            //modelBuilder.Entity<PatternCategory>()
            //    .HasOne<MusicPattern>()
            //    .WithMany(mp => mp.PatternCategories)
            //    .HasForeignKey(pc => pc.MusicPatternId);

            //modelBuilder.Entity<PatternCategory>()
            //    .HasOne<SongCategory>()
            //    .WithMany(c => c.PatternCategories)
            //    .HasForeignKey(pc => pc.StationId);

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

            modelBuilder.Entity<AudioMetadata>()
                .HasOne(am => am.Station)
                .WithMany(s => s.AudioFiles)
                .HasForeignKey(am => am.StationId);

            modelBuilder.Entity<AudioMetadata>()
                .HasOne(am => am.Category)
                .WithMany()
                .HasForeignKey(am => am.CategoryId); // No changes needed here

            // Configure MusicGridItem entity
            modelBuilder.Entity<MusicGridItem>()
                .HasKey(mgi => mgi.Id);

            // Add unique constraint to AppSettings Id
            modelBuilder.Entity<AppSettings>()
                .HasIndex(a => a.Id)
                .IsUnique();

            // Remove seed data for AppSettings
            modelBuilder.Entity<AppSettings>().HasData(
                new AppSettings
                {
                    Id = 1,
                    DataPath = "C:\\Program Files\\Audionix\\AudionixAudio",
                    IsDatapathSetup = false
                }
            );

            // Add unique constraint to SongScheduleSettings Id
            modelBuilder.Entity<SongScheduleSettings>()
                .HasIndex(a => a.Id)
                .IsUnique();

            // Remove seed data for SongScheduleSettings
            modelBuilder.Entity<SongScheduleSettings>().HasData(
                new SongScheduleSettings
                {
                    Id = 1,
                    ArtistSeperation = 10
                }
            );

            // Configure composite key for ProgramLogItem
            modelBuilder.Entity<ProgramLogItem>()
                .HasKey(pl => new { pl.Date, pl.LogOrderID });

        }
    }
}
