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
                .HasOne(pc => pc.SongCategory)
                .WithMany(c => c.PatternCategories)
                .HasForeignKey(pc => pc.SongCategoryId);

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

            // Configure MusicGridItem entity
            modelBuilder.Entity<MusicGridItem>()
                .HasKey(mgi => mgi.Id);

            // Add unique constraint to AppSettings Id
            modelBuilder.Entity<AppSettings>()
                .HasIndex(a => a.Id)
                .IsUnique();

            // Seed data for AppSettings
            modelBuilder.Entity<AppSettings>().HasData(
                new AppSettings
                {
                    Id = 1,
                    DataPath = "C:\\Program Files\\Audionix\\AudionixAudio",
                    IsDatapathSetup = false
                }
            );

            // Configure composite key for ProgramLogItem
            modelBuilder.Entity<ProgramLogItem>()
                .HasKey(pl => new { pl.Date, pl.LogOrderID });

            // Seed data for Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Current" },
                new Category { Id = 2, Name = "Recurrent" },
                new Category { Id = 3, Name = "Gold" },
                new Category { Id = 4, Name = "Power" },
                new Category { Id = 5, Name = "Specialty" },
                new Category { Id = 6, Name = "Holiday" },
                new Category { Id = 7, Name = "Liner" },
                new Category { Id = 8, Name = "Custom" },
                new Category { Id = 9, Name = "None" }
                );

            // Seed data for SoundCodes
            modelBuilder.Entity<SoundCode>().HasData(
                new SoundCode { Id = 1, Code = "M", Description = "Music" },
                new SoundCode { Id = 2, Code = "S", Description = "Sweep" },
                new SoundCode { Id = 3, Code = "V", Description = "Voice" },
                new SoundCode { Id = 4, Code = "P", Description = "Promo" },
                new SoundCode { Id = 5, Code = "L", Description = "Liner" },
                new SoundCode { Id = 6, Code = "I", Description = "ID" },
                new SoundCode { Id = 7, Code = "J", Description = "Jingle" },
                new SoundCode { Id = 8, Code = "C", Description = "Commercial" },
                new SoundCode { Id = 9, Code = "E", Description = "Element" },
                new SoundCode { Id = 10, Code = "X", Description = "Unknown" }
                );

            // Seed data for EnergyLevels
            modelBuilder.Entity<EnergyLevel>().HasData(
                new EnergyLevel { Id = 1, Level = "Low" },
                new EnergyLevel { Id = 2, Level = "Medium" },
                new EnergyLevel { Id = 3, Level = "High" }
                );
        }
    }
}
