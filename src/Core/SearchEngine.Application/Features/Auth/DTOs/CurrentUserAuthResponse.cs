namespace SearchEngine.Application.Features.Auth.DTOs;

public class CurrentUserAuthResponse
{
    public string Id { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public bool IsActive { get; set; }
    public bool IsSuperUser { get; set; }
    public List<string> Roles { get; set; } = [];
    public List<ReadMenuResponse> Menus { get; set; } = [];
    public List<ReadPermissionAuthResponse> Permissions { get; set; } = [];
}


