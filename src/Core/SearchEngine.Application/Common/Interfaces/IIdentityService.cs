using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.Auth.DTOs;
using SearchEngine.Application.Features.Users.DTOs;

namespace SearchEngine.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<Result<ReadUserResponse>> CreateUserAsync(
        CreateUserRequest request);

    Task<Result<ReadUserResponse>> UpdateUserAsync(
        string id,
        UpdateUserRequest request);

    Task<Result<string>> DeleteUserAsync(
        string id);

    Task<Result<string>> ToggleUserStatusAsync(
        string id);

    Task<Result<ReadUserResponse>> GetUserByIdAsync(
        string id);

    Task<Result<PaginatedResult<ReadUserResponse>>>
        GetAllUsersAsync(
            PaginationRequest request);

    // =========================
    // AUTH
    // =========================

    Task<Result<AuthResponse>> LoginAsync(
        string email,
        string password);

    Task<Result<AuthResponse>> RefreshTokenAsync(
        string refreshToken);

    Task<Result<CurrentUserAuthResponse>>
        GetCurrentUserAsync(
            string userId);

    Task<Result<ReadUserResponse>> UpdateProfileAsync(
        string userId,
        UpdateProfileRequest request);

    Task<Result<string>> ChangePasswordAsync(
        string userId,
        ChangePasswordRequest request);

    Task<Result<string>> LogoutAsync(
        string userId);
}
