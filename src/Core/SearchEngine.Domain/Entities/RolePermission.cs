using SearchEngine.Domain.Common;

namespace SearchEngine.Domain.Entities;

/// <summary>
/// Relasi antara Role (RoleId → IdentityRole) dan master Permission.
/// Satu baris = satu permission yang diberikan kepada sebuah role.
/// </summary>
public class RolePermission
    : BaseAuditableEntity
{
    public string RoleId { get; set; } = default!;

    public Guid PermissionId { get; set; }

    public Permission Permission { get; set; } = default!;
}
