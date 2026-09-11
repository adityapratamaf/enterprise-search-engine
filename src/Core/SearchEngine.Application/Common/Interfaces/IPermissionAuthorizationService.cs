namespace SearchEngine.Application.Common.Interfaces;

/// <summary>
/// Mengevaluasi apakah user saat ini memiliki izin pada sebuah module/action
/// secara imperatif (dipakai untuk otorisasi berbasis resource, mis. file
/// attachment yang module pemiliknya baru diketahui saat runtime).
/// </summary>
public interface IPermissionAuthorizationService
{
    Task<bool> HasPermissionAsync(
        string module,
        string action);
}
