using Microsoft.EntityFrameworkCore;

namespace Audionix.Data
{
    public interface IDbContextFactory
    {
        ApplicationDbContext CreateDbContext();
    }

    public class DbContextFactory : IDbContextFactory
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public DbContextFactory(DbContextOptions<ApplicationDbContext> options)
        {
            _options = options;
        }

        public ApplicationDbContext CreateDbContext()
        {
            return new ApplicationDbContext(_options);
        }
    }

}
