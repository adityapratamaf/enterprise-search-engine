using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.Auth.DTOs;
using SearchEngine.Application.Features.Users.DTOs;
using SearchEngine.Infrastructure.Identity.Context;
using SearchEngine.Infrastructure.Identity.Entities;
using SearchEngine.Infrastructure.Identity.Security;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace SearchEngine.Infrastructure.Identity.Services;

public class AuthService
{
    private readonly UserManager<ApplicationUser>
        _userManager;

    private readonly SignInManager<ApplicationUser>
        _signInManager;

    private readonly IJwtService
        _jwtService;

    private readonly ApplicationIdentityDbContext
        _context;

    private readonly PermissionService
        _permissionService;

    private readonly IHttpContextAccessor
        _httpContextAccessor;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtService jwtService,
        ApplicationIdentityDbContext context,
        PermissionService permissionService,
        IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _context = context;
        _permissionService = permissionService;
        _httpContextAccessor = httpContextAccessor;
    }

    // ========== Login ==========
    public async Task<Result<AuthResponse>>
    LoginAsync(
        string email,
        string password)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(
                x => x.Email == email);

        if (user is null)
        {
            return Result<AuthResponse>
                .Failure("Invalid credentials");
        }

        var signInResult =
            await _signInManager
                .CheckPasswordSignInAsync(
                    user,
                    password,
                    true);

        if (signInResult.IsLockedOut)
        {
            return Result<AuthResponse>
                .Failure("User account is locked.");
        }

        if (signInResult.IsNotAllowed)
        {
            return Result<AuthResponse>
                .Failure("User is not allowed to login.");
        }

        if (signInResult.RequiresTwoFactor)
        {
            return Result<AuthResponse>
                .Failure("Two-factor authentication is required.");
        }

        if (!signInResult.Succeeded)
        {
            return Result<AuthResponse>
                .Failure("Invalid credentials");
        }

        if (!user.IsActive)
        {
            return Result<AuthResponse>
                .Failure("User account is inactive.");
        }

        var roles =
            (await _userManager
                .GetRolesAsync(user))
            .ToList();

        return await _jwtService
            .GenerateTokenAsync(
                user.Id,
                user.UserName!,
                user.Email!,
                roles,
                user.IsActive,
                user.IsSuperUser,
                user.FirstName,
                user.LastName);
    }

    // ========== Logout ==========
    public async Task<Result<string>>
    LogoutAsync(
        string userId)
    {
        var ipAddress =
            ClientIpAccessor.GetIpAddress(
                _httpContextAccessor);

        var refreshTokens =
            await _context.RefreshTokens
                .Where(x =>
                    x.UserId == userId &&
                    !x.IsRevoked)
                .ToListAsync();

        foreach (var token in refreshTokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedByIp = ipAddress;
        }

        await _context.SaveChangesAsync();

        return Result<string>
            .SuccessResult(
                string.Empty,
                "Logout success");
    }

    // ========== Get Current User ==========
    public async Task<
    Result<CurrentUserAuthResponse>>
    GetCurrentUserAsync(
        string userId)
    {
        var user =
            await _userManager.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == userId);

        if (user is null)
        {
            return Result<CurrentUserAuthResponse>
                .Failure(
                    "User not found");
        }

        var roles =
            await _userManager
                .GetRolesAsync(user);

        var (menus, permissions) =
            await _permissionService
                .GetMenusAndPermissionsAsync(roles);

        var response =
            new CurrentUserAuthResponse
            {
                Id = user.Id,
                Username = user.UserName!,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.IsActive,
                IsSuperUser = user.IsSuperUser,
                Roles = roles.ToList(),

                Menus = menus,

                Permissions = permissions
            };

        return Result<CurrentUserAuthResponse>
            .SuccessResult(
                response,
                "Profile retrieved successfully.");
    }

    // ========== Update Profile ==========
    public async Task<Result<ReadUserResponse>>
    UpdateProfileAsync(
        string userId,
        UpdateProfileRequest request)
    {
        var user = await _userManager
            .FindByIdAsync(userId);

        if (user is null)
        {
            return Result<ReadUserResponse>
                .Failure("User not found");
        }

        // Username unik, mengabaikan milik user saat ini.
        var usernameTaken = await _userManager.Users
            .AnyAsync(x =>
                x.UserName == request.Username &&
                x.Id != userId);

        if (usernameTaken)
        {
            return Result<ReadUserResponse>
                .Failure("Username already exists");
        }

        // Email unik, mengabaikan milik user saat ini.
        var emailTaken = await _userManager.Users
            .AnyAsync(x =>
                x.Email == request.Email &&
                x.Id != userId);

        if (emailTaken)
        {
            return Result<ReadUserResponse>
                .Failure("Email already exists");
        }

        // Hanya field profil yang boleh diubah.
        // Role, IsActive, dan IsSuperUser tidak disentuh.
        user.UserName = request.Username;
        user.Email = request.Email;
        user.FirstName = request.FirstName!;
        user.LastName = request.LastName!;

        var updateResult =
            await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            return Result<ReadUserResponse>
                .Failure(
                    "Failed to update profile",
                    updateResult.Errors);
        }

        var roles = await _userManager
            .GetRolesAsync(user);

        var dto = user.Adapt<ReadUserResponse>();

        dto.Role =
            roles.FirstOrDefault()
            ?? "-";

        return Result<ReadUserResponse>
            .SuccessResult(
                dto,
                "Profile updated successfully");
    }

    // ========== Change Password ==========
    public async Task<Result<string>>
    ChangePasswordAsync(
        string userId,
        ChangePasswordRequest request)
    {
        var user = await _userManager
            .FindByIdAsync(userId);

        if (user is null)
        {
            return Result<string>
                .Failure("User not found.");
        }

        // Password baru tidak boleh sama dengan password saat ini.
        if (request.CurrentPassword == request.NewPassword)
        {
            return Result<string>
                .Failure(
                    "New password cannot be the same as current password.");
        }

        // Verifikasi current password sekaligus mengganti password
        // menggunakan ASP.NET Identity (password policy ikut divalidasi).
        var result = await _userManager
            .ChangePasswordAsync(
                user,
                request.CurrentPassword!,
                request.NewPassword!);

        if (!result.Succeeded)
        {
            if (result.Errors.Any(x =>
                    x.Code == "PasswordMismatch"))
            {
                return Result<string>
                    .Failure(
                        "Current password is incorrect.");
            }

            return Result<string>
                .Failure(
                    "Failed to change password",
                    result.Errors);
        }

        return Result<string>
            .SuccessResult(
                string.Empty,
                "Password changed successfully.");
    }
}
