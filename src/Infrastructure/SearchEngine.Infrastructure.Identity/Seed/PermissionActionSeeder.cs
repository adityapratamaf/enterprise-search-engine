using SearchEngine.Domain.Entities;
using SearchEngine.Infrastructure.Identity.Context;
using Microsoft.EntityFrameworkCore;

namespace SearchEngine.Infrastructure.Identity.Seed;

public static class PermissionActionSeeder
{
    public static async Task SeedAsync(
        ApplicationIdentityDbContext context)
    {
        var actions = new List<PermissionAction>
        {
            new() { Id = Guid.NewGuid(), Code = "View",     Name = "View",     DisplayOrder = 1,  IsActive = true },
            new() { Id = Guid.NewGuid(), Code = "Create",   Name = "Create",   DisplayOrder = 2,  IsActive = true },
            new() { Id = Guid.NewGuid(), Code = "Update",   Name = "Update",   DisplayOrder = 3,  IsActive = true },
            new() { Id = Guid.NewGuid(), Code = "Delete",   Name = "Delete",   DisplayOrder = 4,  IsActive = true },
            new() { Id = Guid.NewGuid(), Code = "Approve",  Name = "Approve",  DisplayOrder = 5,  IsActive = true },
            new() { Id = Guid.NewGuid(), Code = "Download", Name = "Download", DisplayOrder = 6,  IsActive = true },
            new() { Id = Guid.NewGuid(), Code = "Export",   Name = "Export",   DisplayOrder = 7,  IsActive = true },
            new() { Id = Guid.NewGuid(), Code = "Import",   Name = "Import",   DisplayOrder = 8,  IsActive = true },
            new() { Id = Guid.NewGuid(), Code = "Print",    Name = "Print",    DisplayOrder = 9,  IsActive = true },
            new() { Id = Guid.NewGuid(), Code = "Assign",   Name = "Assign",   DisplayOrder = 10, IsActive = true },
            new() { Id = Guid.NewGuid(), Code = "Execute",  Name = "Execute",  DisplayOrder = 11, IsActive = true },
            new() { Id = Guid.NewGuid(), Code = "Archive",  Name = "Archive",  DisplayOrder = 12, IsActive = true },
            new() { Id = Guid.NewGuid(), Code = "Restore",  Name = "Restore",  DisplayOrder = 13, IsActive = true }
        };

        var existingCodes =
            (await context.PermissionActions
                .Select(x => x.Code)
                .ToListAsync())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var missing =
            actions
                .Where(x =>
                    !existingCodes.Contains(x.Code))
                .ToList();

        if (missing.Count == 0)
        {
            return;
        }

        context.PermissionActions.AddRange(missing);

        await context.SaveChangesAsync();
    }
}
