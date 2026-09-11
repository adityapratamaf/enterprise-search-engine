namespace SearchEngine.Application.Features.RolePermissions.DTOs;

public class RolePermissionActionResponse
{
    public Guid ActionId { get; set; }

    public string ActionName { get; set; } = default!;

    public bool Granted { get; set; }
}
