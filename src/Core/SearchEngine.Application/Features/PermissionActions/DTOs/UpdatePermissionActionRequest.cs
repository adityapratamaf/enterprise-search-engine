namespace SearchEngine.Application.Features.PermissionActions.DTOs;

// Code sengaja TIDAK disertakan: Code bersifat immutable setelah dibuat
// (identifier otorisasi + dasar Permission.Code pada katalog).
public class UpdatePermissionActionRequest
{
    public string Name { get; set; } = default!;

    public string? Description { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }
}
