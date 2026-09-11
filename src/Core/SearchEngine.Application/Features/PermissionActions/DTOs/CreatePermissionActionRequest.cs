namespace SearchEngine.Application.Features.PermissionActions.DTOs;

public class CreatePermissionActionRequest
{
    public string Code { get; set; } = default!;

    public string Name { get; set; } = default!;

    public string? Description { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;
}
