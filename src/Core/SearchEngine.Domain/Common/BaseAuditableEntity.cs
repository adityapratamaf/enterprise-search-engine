namespace SearchEngine.Domain.Common;

public abstract class BaseAuditableEntity
    : BaseEntity,
      IAuditableEntity
{
    public DateTime CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? DeletedAt { get; set; }

    public string? DeletedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public string? ApprovedBy { get; set; }

    public bool IsDeleted { get; set; }

    public bool IsApproved { get; set; }
}
