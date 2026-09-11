namespace SearchEngine.Application.Features.Auth.DTOs;

public class UpdateProfileRequest
{
    public string? Username { get; set; }

    public string? Email { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }
}
