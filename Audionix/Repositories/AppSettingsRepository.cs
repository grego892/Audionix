using Audionix.Data;
using Audionix.Models;
using Microsoft.EntityFrameworkCore;

namespace Audionix.Repositories
{
    public class AppSettingsRepository : IAppSettingsRepository
    {
        private readonly IDbContextFactory _dbContextFactory;

        public AppSettingsRepository(IDbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<AppSettings?> GetAppSettingsAsync()
        {
            using var context = _dbContextFactory.CreateDbContext();
            return await context.AppSettings.FirstOrDefaultAsync();
        }

        public async Task SaveAppSettingsAsync(AppSettings appSettings)
        {
            using var context = _dbContextFactory.CreateDbContext();
            context.AppSettings.Update(appSettings);
            await context.SaveChangesAsync();
        }
    }
}
