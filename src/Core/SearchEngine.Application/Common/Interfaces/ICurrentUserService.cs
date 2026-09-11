namespace SearchEngine.Application.Common.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }

    string? UserName { get; }

    string? Email { get; }

    string? Role { get; }

    bool IsSuperUser { get; }
}
