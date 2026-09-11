using Microsoft.AspNetCore.Identity;

namespace SearchEngine.Infrastructure.Identity.Services;

public class RoleService
{
    private readonly RoleManager<IdentityRole>
        _roleManager;

    public RoleService(
        RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public Task<bool> RoleExistsAsync(
        string role)
    {
        return _roleManager
            .RoleExistsAsync(role);
    }
}
