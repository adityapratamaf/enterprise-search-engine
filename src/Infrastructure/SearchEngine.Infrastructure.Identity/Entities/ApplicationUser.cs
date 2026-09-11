using Microsoft.AspNetCore.Identity;

namespace SearchEngine.Infrastructure.Identity.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public bool IsActive { get; set; }
    public bool IsSuperUser { get; set; }
}
