using SearchEngine.Domain.Entities;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Common.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SearchEngine.Infrastructure.Identity.Context;
using SearchEngine.Infrastructure.Identity.Security;

namespace SearchEngine.Infrastructure.Identity.Services;

public class JwtService : IJwtService
{
    private readonly JwtSettings _jwtSettings;

    private readonly ApplicationIdentityDbContext _context;

    private readonly IHttpContextAccessor _httpContextAccessor;

    private readonly PermissionService _permissionService;

    public JwtService(
        IOptions<JwtSettings> jwtSettings,
        IHttpContextAccessor httpContextAccessor,
        ApplicationIdentityDbContext context,
        PermissionService permissionService)
    {
        _jwtSettings = jwtSettings.Value;
        _httpContextAccessor = httpContextAccessor;
        _context = context;
        _permissionService = permissionService;
    }

    public async Task<Result<AuthResponse>> GenerateTokenAsync(
        string userId,
        string username,
        string email,
        List<string> roles,
        bool isActive,
        bool isSuperUser,
        string firstName,
        string lastName)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier,userId),
            new(JwtRegisteredClaimNames.Sub,userId),
            new(JwtRegisteredClaimNames.Email,email),
            new(ClaimTypes.Name,username),
            new("is_superuser",isSuperUser.ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(
                new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSettings.Secret));

        var creds = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var expiry = DateTime.UtcNow.AddMinutes(
            _jwtSettings.AccessTokenExpirationMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiry,
            signingCredentials: creds);

        var accessToken = new JwtSecurityTokenHandler()
            .WriteToken(token);

        var refreshToken = GenerateRefreshToken();

        var modules =
            await _permissionService
                .GetModulePermissionsAsync(roles);

        var response = new AuthResponse
        {
            Access = accessToken,
            Refresh = refreshToken,
            Expiry = new DateTimeOffset(expiry)
                .ToUnixTimeMilliseconds(),
            User = new UserResponse
            {
                Id = userId,
                Username = username,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Role = roles.FirstOrDefault() ?? "Member",
                IsActive = isActive,
                IsSuperUser = isSuperUser
            },

            Modules = modules
        };

        // =====================================================
        // SAVE REFRESH TOKEN
        // =====================================================

        var httpContext =
            _httpContextAccessor.HttpContext;

        var ipAddress =
            ClientIpAccessor.GetIpAddress(
                _httpContextAccessor);

        var userAgent =
            httpContext?
                .Request
                .Headers["User-Agent"]
                .ToString();

        var refreshTokenEntity =
            new RefreshToken
            {
                UserId = userId,
                TokenHash =
                    RefreshTokenHasher.Hash(
                        refreshToken),
                ExpiredAt = DateTime.UtcNow
                    .AddDays(
                        _jwtSettings
                            .RefreshTokenExpirationDays),
                CreatedByIp = ipAddress,
                UserAgent = userAgent,
                IsRevoked = false
            };

        await _context.RefreshTokens.AddAsync(
            refreshTokenEntity);

        await _context.SaveChangesAsync();

        return Result<AuthResponse>
            .SuccessResult(
                response,
                "Login success");
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[32];

        using var rng = RandomNumberGenerator.Create();

        rng.GetBytes(randomBytes);

        return Convert.ToBase64String(randomBytes);
    }
}
