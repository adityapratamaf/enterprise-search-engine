using SearchEngine.Domain.Entities;
using SearchEngine.Infrastructure.Identity.Context;

namespace SearchEngine.Infrastructure.Identity.Seed;

public static class ModuleSeeder
{
    public static async Task SeedAsync(
        ApplicationIdentityDbContext context)
    {
        var modules = new List<Module>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ModuleId = "dashboard",
                ModuleName = "Dashboard",
                ModulePath = "/dashboard"
            },

            new()
            {
                Id = Guid.NewGuid(),
                ModuleId = "role-management",
                ModuleName = "Role Management",
                ModulePath = "/role-management"
            },

            new()
            {
                Id = Guid.NewGuid(),
                ModuleId = "products",
                ModuleName = "Products",
                ModulePath = "/products"
            },

            new()
            {
                Id = Guid.NewGuid(),
                ModuleId = "modules",
                ModuleName = "Modules",
                ModulePath = "/modules"
            },

            new()
            {
                Id = Guid.NewGuid(),
                ModuleId = "roles",
                ModuleName = "Roles",
                ModulePath = "/roles"
            },

            new()
            {
                Id = Guid.NewGuid(),
                ModuleId = "users",
                ModuleName = "Users",
                ModulePath = "/users"
            },

            new()
            {
                Id = Guid.NewGuid(),
                ModuleId = "email",
                ModuleName = "Email",
                ModulePath = "/email"
            },

            new()
            {
                Id = Guid.NewGuid(),
                ModuleId = "audits",
                ModuleName = "Audits",
                ModulePath = "/audits"
            },

            new()
            {
                Id = Guid.NewGuid(),
                ModuleId = "permission-actions",
                ModuleName = "Permission Actions",
                ModulePath = "/permission-actions"
            },

            new()
            {
                Id = Guid.NewGuid(),
                ModuleId = "search",
                ModuleName = "Search",
                ModulePath = "/search"
            },

            new()
            {
                Id = Guid.NewGuid(),
                ModuleId = "settings",
                ModuleName = "Settings",
                ModulePath = "/settings"
            }

        };

        var existingModuleIds =
            context.Modules
                .Select(x => x.ModuleId)
                .ToHashSet();

        var missingModules =
            modules
                .Where(x =>
                    !existingModuleIds.Contains(
                        x.ModuleId))
                .ToList();

        if (missingModules.Count == 0)
        {
            return;
        }

        context.Modules.AddRange(missingModules);

        await context.SaveChangesAsync();
    }
}
