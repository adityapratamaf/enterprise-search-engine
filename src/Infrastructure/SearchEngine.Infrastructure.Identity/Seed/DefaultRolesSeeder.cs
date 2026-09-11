using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SearchEngine.Infrastructure.Identity.Entities;

namespace SearchEngine.Infrastructure.Identity.Seed;

public static class DefaultRolesSeeder
{
    public static async Task SeedAsync(
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager)
    {
        string[] roles =
        [
            "SuperAdmin",
            "Admin",
            "Member"
        ];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var adminEmail = "admin@searchengine.local";

        var adminUser = await userManager.Users
            .FirstOrDefaultAsync(x => x.Email == adminEmail);

        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = "admin",
                Email = adminEmail,
                FirstName = "Super",
                LastName = "Admin",
                EmailConfirmed = true,
                IsActive = true,
                IsSuperUser = true
            };

            await userManager.CreateAsync(adminUser, "Admin123!");
            await userManager.AddToRoleAsync(adminUser, "SuperAdmin");
        }
    }
}
