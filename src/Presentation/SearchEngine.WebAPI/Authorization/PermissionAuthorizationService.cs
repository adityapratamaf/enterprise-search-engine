using SearchEngine.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace SearchEngine.WebAPI.Authorization;

/// <summary>
/// Adapter otorisasi imperatif: menurunkan pengecekan izin ke policy dinamis
/// "{module}.{action}" yang sudah dibangun oleh PermissionPolicyProvider +
/// PermissionHandler. Dengan begitu tidak ada duplikasi logika permission.
/// </summary>
public class PermissionAuthorizationService
    : IPermissionAuthorizationService
{
    private readonly IAuthorizationService
        _authorizationService;

    private readonly IHttpContextAccessor
        _httpContextAccessor;

    public PermissionAuthorizationService(
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor)
    {
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<bool> HasPermissionAsync(
        string module,
        string action)
    {
        var user =
            _httpContextAccessor.HttpContext?.User;

        if (user is null
            || string.IsNullOrWhiteSpace(module)
            || string.IsNullOrWhiteSpace(action))
        {
            return false;
        }

        var result =
            await _authorizationService.AuthorizeAsync(
                user,
                $"{module}.{action}");

        return result.Succeeded;
    }
}
