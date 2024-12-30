using Microsoft.EntityFrameworkCore;
using SharedLibrary.Data;

namespace SharedLibrary.Data
{
    public class DbContextFactory : IDbContextFactory<ApplicationDbContext>
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

