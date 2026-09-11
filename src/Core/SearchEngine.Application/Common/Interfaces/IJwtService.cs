using SearchEngine.Application.Common.Models;

namespace SearchEngine.Application.Common.Interfaces;

public interface IJwtService
{
    Task<Result<AuthResponse>> GenerateTokenAsync(
        string userId,
        string username,
        string email,
        List<string> roles,
        bool isActive,
        bool isSuperUser,
        string firstName,
        string lastName);

    string GenerateRefreshToken();
}
