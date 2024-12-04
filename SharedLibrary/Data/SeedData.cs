using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;


namespace SharedLibrary.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider, UserManager<ApplicationUser> userManager)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                // Look for any users.
                if (context.Users.Any())
                {
                    return;   // DB has been seeded
                }

                var testUser = new ApplicationUser
                {
                    UserName = "djgrego@djgrego.com",
                    Email = "djgrego@djgrego.com",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(testUser, "Teamone74#");

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        Log.Error("Error creating test user: {Code} - {Description}", error.Code, error.Description);
                    }
                    throw new Exception("Failed to create test user");
                }
            }
        }
    }
}
