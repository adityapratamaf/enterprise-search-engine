using System.Security.Claims;
using SearchEngine.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace SearchEngine.Infrastructure.Persistence.Services;

public class CurrentUserService
    : ICurrentUserService
{
    private readonly IHttpContextAccessor
        _httpContextAccessor;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor =
            httpContextAccessor;
    }

    private ClaimsPrincipal? User =>
        _httpContextAccessor
            .HttpContext?
            .User;

    public string? UserId =>
    _httpContextAccessor
        .HttpContext?
        .User?
        .FindFirstValue(
            ClaimTypes.NameIdentifier);

    public string? UserName =>
        _httpContextAccessor
            .HttpContext?
            .User?
            .FindFirstValue(
                ClaimTypes.Name);

    public string? Email =>
        _httpContextAccessor
            .HttpContext?
            .User?
            .FindFirstValue(
                ClaimTypes.Email);

    public string? Role =>
        _httpContextAccessor
            .HttpContext?
            .User?
            .FindFirstValue(
                ClaimTypes.Role);

    public bool IsSuperUser =>
        bool.TryParse(
            _httpContextAccessor
                .HttpContext?
                .User?
                .FindFirst("is_superuser")
                ?.Value,
            out var result)
                && result;

}
