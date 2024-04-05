using Audionix.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static System.Collections.Specialized.BitVector32;

namespace Audionix.Models
{
    public class AudionixDbContext(DbContextOptions<AudionixDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Station> Stations { get; set; }
        public DbSet<MusicPattern> MusicPatterns { get; set; }
        public DbSet<MusicPatternData> MusicPatternsData { get; set; }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    if (AppSettings.Settings.Datapath != null)
        //    {
        //        string databasePath = Path.Combine(AppSettings.Settings.Datapath, "System", "Database", "Audionix.db");
        //        optionsBuilder.UseSqlite($"Data Source={databasePath}");
        //    }
        //    else
        //    {
        //        // Fallback to the default location if settings.Datapath is not set
        //        optionsBuilder.UseSqlite($"Data Source={Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Audionix", "Audionix.db")}");
        //    }
        //}
    }
}
