using SearchEngine.Domain.Common;

namespace SearchEngine.Domain.Entities;

public class PermissionAction
    : BaseAuditableEntity
{
    public string Code { get; set; } = default!;

    public string Name { get; set; } = default!;

    public string? Description { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;
}
