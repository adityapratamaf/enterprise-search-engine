namespace SearchEngine.Application.Features.RolePermissions.DTOs;

public class AssignModulePermissionRequest
{
    public string RoleName { get; set; } = default!;

    public Guid ModuleId { get; set; }

    public List<Guid> ActionIds { get; set; } = [];
}
