namespace SearchEngine.Domain.Common;

public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }

    string? CreatedBy { get; set; }

    DateTime? UpdatedAt { get; set; }

    string? UpdatedBy { get; set; }

    DateTime? DeletedAt { get; set; }

    string? DeletedBy { get; set; }

    DateTime? ApprovedAt { get; set; }

    string? ApprovedBy { get; set; }

    bool IsDeleted { get; set; }

    bool IsApproved { get; set; }
}
