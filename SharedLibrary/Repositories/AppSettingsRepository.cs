using SharedLibrary.Data;
using SharedLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace SharedLibrary.Repositories
{
    public class AppSettingsRepository : IAppSettingsRepository
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public AppSettingsRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<AppSettings?> GetAppSettingsAsync()
        {
            try
            {
                using var context = _dbContextFactory.CreateDbContext();
                return await context.AppSettings.FirstOrDefaultAsync();
            }
            catch (Npgsql.NpgsqlException ex)
            {
                Log.Error(ex, "++++++ AppSettingsRepository.cs - PostgreSQL is not running or not set up correctly.");
                return null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "++++++ AppSettingsRepository.cs - An error occurred while getting app settings.");
                return null;
            }
        }

        public async Task SaveAppSettingsAsync(AppSettings appSettings)
        {
            try
            {
                using var context = _dbContextFactory.CreateDbContext();
                context.AppSettings.Update(appSettings);
                await context.SaveChangesAsync();
            }
            catch (Npgsql.NpgsqlException ex)
            {
                Log.Error(ex, "++++++ AppSettingsRepository.cs - PostgreSQL is not running or not set up correctly.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "++++++ AppSettingsRepository.cs - An error occurred while saving app settings.");
            }
        }
    }
}

