namespace SearchEngine.Application.Features.RolePermissions.DTOs;

public class ReadRolePermissionResponse
{
    public Guid ModuleId { get; set; }

    public string ModuleName { get; set; } = default!;

    public List<RolePermissionActionResponse> Permissions { get; set; } = [];
}
