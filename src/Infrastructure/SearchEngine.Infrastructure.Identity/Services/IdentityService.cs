using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.Users.DTOs;
using SearchEngine.Application.Features.Auth.DTOs;

namespace SearchEngine.Infrastructure.Identity.Services;

public class IdentityService
    : IIdentityService
{
    private readonly UserService
        _userService;

    private readonly AuthService
        _authService;

    private readonly RefreshTokenService
        _refreshTokenService;

    public IdentityService(
        UserService userService,
        AuthService authService,
        RefreshTokenService refreshTokenService)
    {
        _userService = userService;
        _authService = authService;
        _refreshTokenService = refreshTokenService;
    }

    // ========== Create ==========
    public Task<Result<ReadUserResponse>>
        CreateUserAsync(
            CreateUserRequest request)
        => _userService.CreateUserAsync(request);

    // ========== Get All ==========
    public Task<
        Result<
            PaginatedResult<ReadUserResponse>>>
        GetAllUsersAsync(
            PaginationRequest request)
        => _userService.GetAllUsersAsync(request);

    // ========== Get By Id ==========
    public Task<Result<ReadUserResponse>>
        GetUserByIdAsync(
            string id)
        => _userService.GetUserByIdAsync(id);

    // ========== Update ==========
    public Task<Result<ReadUserResponse>>
        UpdateUserAsync(
            string id,
            UpdateUserRequest request)
        => _userService.UpdateUserAsync(id, request);

    // ========== Delete ==========
    public Task<Result<string>>
        DeleteUserAsync(
            string id)
        => _userService.DeleteUserAsync(id);

    // ========== Toogle ==========
    public Task<Result<string>>
        ToggleUserStatusAsync(
            string id)
        => _userService.ToggleUserStatusAsync(id);

    // ========== Login ==========
    public Task<Result<AuthResponse>>
        LoginAsync(
            string email,
            string password)
        => _authService.LoginAsync(email, password);

    // ========== Logout ==========
    public Task<Result<string>>
        LogoutAsync(
            string userId)
        => _authService.LogoutAsync(userId);

    // ========== Get Current User ==========
    public Task<Result<CurrentUserAuthResponse>>
        GetCurrentUserAsync(
            string userId)
        => _authService.GetCurrentUserAsync(userId);

    // ========== Update Profile ==========
    public Task<Result<ReadUserResponse>>
        UpdateProfileAsync(
            string userId,
            UpdateProfileRequest request)
        => _authService.UpdateProfileAsync(userId, request);

    // ========== Change Password ==========
    public Task<Result<string>>
        ChangePasswordAsync(
            string userId,
            ChangePasswordRequest request)
        => _authService.ChangePasswordAsync(userId, request);

    // ========== Refresh Token ==========
    public Task<Result<AuthResponse>>
        RefreshTokenAsync(
            string refreshTokenValue)
        => _refreshTokenService.RefreshTokenAsync(refreshTokenValue);
}
