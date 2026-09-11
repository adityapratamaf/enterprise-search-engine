namespace SearchEngine.Application.Features.Users.DTOs;

public class ReadUserResponse
{
    public string Id { get; set; } = default!;

    public string Username { get; set; } = default!;

    public string Email { get; set; } = default!;

    public string FirstName { get; set; } = default!;

    public string LastName { get; set; } = default!;

    public bool IsActive { get; set; }

    public bool IsSuperUser { get; set; }

    public string Role { get; set; } = default!;
}
