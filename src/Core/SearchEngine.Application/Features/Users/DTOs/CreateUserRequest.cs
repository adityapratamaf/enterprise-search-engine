namespace SearchEngine.Application.Features.Users.DTOs;

public class CreateUserRequest
{
    public string? Username { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Role { get; set; }

    public bool IsSuperUser { get; set; }
}
