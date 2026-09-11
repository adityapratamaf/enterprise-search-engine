namespace SearchEngine.Application.Common.Security;

/// <summary>
/// Konstanta aksi audit untuk event keamanan. Menghindari magic string
/// dan menjaga konsistensi pencatatan pada sistem Audit yang sudah ada.
/// </summary>
public static class SecurityAuditAction
{
    public const string Module = "Security";

    public const string LoginSuccess = "LOGIN_SUCCESS";
    public const string LoginFailed = "LOGIN_FAILED";
    public const string UserLocked = "USER_LOCKED";

    public const string RefreshTokenSuccess = "REFRESH_TOKEN_SUCCESS";
    public const string RefreshTokenFailed = "REFRESH_TOKEN_FAILED";

    public const string Logout = "LOGOUT";

    public const string RoleChanged = "ROLE_CHANGED";
    public const string PermissionChanged = "PERMISSION_CHANGED";
}
