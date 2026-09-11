using SearchEngine.Domain.Common;

namespace SearchEngine.Domain.Entities;

public class Product
    : BaseAuditableEntity
{
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? Description { get; set; }
}
