using SearchEngine.Domain.Common;

namespace SearchEngine.Domain.Entities;

/// <summary>
/// Master permission: satu baris mewakili satu kombinasi Module + Action
/// yang valid (mis. "users.view"). Role diberi izin melalui RolePermission
/// yang mereferensikan Permission ini.
/// </summary>
public class Permission
    : BaseAuditableEntity
{
    public Guid ModuleId { get; set; }

    public Module Module { get; set; } = default!;

    public Guid PermissionActionId { get; set; }

    public PermissionAction PermissionAction { get; set; } = default!;

    /// <summary>
    /// Kode unik permission, mis. "users.view".
    /// </summary>
    public string Code { get; set; } = default!;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}
