using SearchEngine.Domain.Common;

namespace SearchEngine.Domain.Entities;

public class Module
    : BaseAuditableEntity
{
    public string ModuleId { get; set; } = default!;

    public string ModuleName { get; set; } = default!;

    public string ModulePath { get; set; } = default!;
}
