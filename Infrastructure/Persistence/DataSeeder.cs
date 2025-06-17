using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MyApp.Infrastructure.Persistence
{
    public class DataSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<DataSeeder>>();

            try
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser<Guid>>>();

                // 1. Seed roles
                string[] roles = new[] { "Admin", "User" };
                foreach (var roleName in roles)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        var role = new IdentityRole<Guid>(roleName);
                        await roleManager.CreateAsync(role);
                        logger.LogInformation("Seeded role '{Role}'", roleName);
                    }
                }

                // 2. Seed default admin user
                var adminEmail = "admin@example.com";
                var adminPassword = "Admin@1234"; // Có thể lấy từ config hoặc user secrets
                if (await userManager.FindByEmailAsync(adminEmail) == null)
                {
                    var adminUser = new IdentityUser<Guid>
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true
                    };
                    var result = await userManager.CreateAsync(adminUser, adminPassword);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                        logger.LogInformation("Seeded user '{Email}' as Admin", adminEmail);
                    }
                    else
                    {
                        var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                        logger.LogError("Failed to create Admin user: {Errors}", errors);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database.");
                throw;
            }
        }
    }
}
