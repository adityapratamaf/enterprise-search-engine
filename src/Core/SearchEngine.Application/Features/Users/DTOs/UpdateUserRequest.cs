namespace SearchEngine.Application.Features.Users.DTOs;

public class UpdateUserRequest
{
    public string? Username { get; set; }

    public string? Email { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Role { get; set; }

    public bool IsActive { get; set; }

    public bool IsSuperUser { get; set; }
}
