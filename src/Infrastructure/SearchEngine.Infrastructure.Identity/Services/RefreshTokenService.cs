using SearchEngine.Application.Common.Models;
using SearchEngine.Infrastructure.Identity.Context;
using SearchEngine.Infrastructure.Identity.Entities;
using SearchEngine.Infrastructure.Identity.Security;
using SearchEngine.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace SearchEngine.Infrastructure.Identity.Services;

public class RefreshTokenService
{
    private readonly UserManager<ApplicationUser>
        _userManager;

    private readonly IJwtService
        _jwtService;

    private readonly ApplicationIdentityDbContext
        _context;

    private readonly IHttpContextAccessor
        _httpContextAccessor;

    public RefreshTokenService(
        UserManager<ApplicationUser> userManager,
        IJwtService jwtService,
        ApplicationIdentityDbContext context,
        IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _jwtService = jwtService;
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    // Refresh Token — single-use with atomic consumption.
    public async Task<Result<AuthResponse>>
    RefreshTokenAsync(
        string refreshTokenValue)
    {
        var tokenHash =
            RefreshTokenHasher.Hash(
                refreshTokenValue);

        var ipAddress =
            ClientIpAccessor.GetIpAddress(
                _httpContextAccessor);

        var now = DateTime.UtcNow;

        var refreshToken =
            await _context.RefreshTokens
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.TokenHash ==
                        tokenHash);

        if (refreshToken is null)
        {
            return Result<AuthResponse>
                .Failure(
                    "Invalid refresh token");
        }

        // Reuse detection: a revoked token was presented again -> revoke every
        // active token for that user (atomic set-based update).
        if (refreshToken.IsRevoked)
        {
            await _context.RefreshTokens
                .Where(x =>
                    x.UserId == refreshToken.UserId
                    && !x.IsRevoked)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.IsRevoked, true)
                    .SetProperty(x => x.RevokedAt, now)
                    .SetProperty(x => x.RevokedByIp, ipAddress));

            return Result<AuthResponse>
                .Failure(
                    "Refresh token reuse detected");
        }

        if (refreshToken.ExpiredAt < now)
        {
            return Result<AuthResponse>
                .Failure(
                    "Refresh token expired");
        }

        // Atomic single-use consumption: only ONE concurrent request can flip
        // IsRevoked from false to true. The database applies this as a single
        // UPDATE ... WHERE TokenHash = @h AND IsRevoked = 0 AND ExpiredAt > @now.
        var consumed =
            await _context.RefreshTokens
                .Where(x =>
                    x.TokenHash == tokenHash
                    && !x.IsRevoked
                    && x.ExpiredAt > now)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.IsRevoked, true)
                    .SetProperty(x => x.RevokedAt, now)
                    .SetProperty(x => x.RevokedByIp, ipAddress)
                    .SetProperty(x => x.LastUsedAt, now));

        if (consumed == 0)
        {
            // Lost the race: another concurrent request already consumed the
            // token (or it was revoked/expired in the meantime).
            return Result<AuthResponse>
                .Failure(
                    "Refresh token already used");
        }

        var user =
            await _userManager.Users
                .FirstOrDefaultAsync(
                    x => x.Id ==
                        refreshToken.UserId);

        if (user is null)
        {
            return Result<AuthResponse>
                .Failure(
                    "User not found");
        }

        var roles =
            (await _userManager
                .GetRolesAsync(user))
            .ToList();

        var token =
            await _jwtService
                .GenerateTokenAsync(
                    user.Id,
                    user.UserName!,
                    user.Email!,
                    roles,
                    user.IsActive,
                    user.IsSuperUser,
                    user.FirstName,
                    user.LastName);

        // Link the consumed token to its replacement (token-chain traceability).
        var replacementHash =
            RefreshTokenHasher.Hash(token.Data!.Refresh);

        await _context.RefreshTokens
            .Where(x => x.TokenHash == tokenHash)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(
                    x => x.ReplacedByTokenHash,
                    replacementHash));

        return token;
    }
}
