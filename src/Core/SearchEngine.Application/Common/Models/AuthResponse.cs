namespace SearchEngine.Application.Common.Models;

public class AuthResponse
{
    public string Access { get; set; } = default!;
    public string Refresh { get; set; } = default!;
    public long Expiry { get; set; }
    public UserResponse User { get; set; } = default!;
    public List<ModulePermissionResponse> Modules { get; set; } = [];
}
