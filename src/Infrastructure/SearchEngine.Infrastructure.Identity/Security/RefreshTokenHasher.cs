using System.Security.Cryptography;
using System.Text;

namespace SearchEngine.Infrastructure.Identity.Security;

/// <summary>
/// Reusable helper untuk hashing Refresh Token.
/// Refresh Token TIDAK PERNAH disimpan dalam bentuk plaintext.
/// Hanya hash SHA256 (hexadecimal, lowercase) yang dipersistensi.
/// </summary>
public static class RefreshTokenHasher
{
    public static string Hash(
        string rawToken)
    {
        var bytes =
            SHA256.HashData(
                Encoding.UTF8.GetBytes(rawToken));

        return Convert.ToHexString(bytes)
            .ToLowerInvariant();
    }
}
