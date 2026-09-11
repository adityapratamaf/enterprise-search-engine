namespace SearchEngine.Infrastructure.Search.Indexing;

/// <summary>
/// Menentukan penamaan alias dan index fisik.
///
/// Aplikasi tidak pernah menyebut index fisik saat mencari; yang dipakai
/// selalu alias. Index fisik diberi akhiran cap waktu sehingga indexing
/// ulang dapat membangun versi baru tanpa mengganggu versi yang sedang
/// melayani, lalu alias dipindahkan dalam satu operasi atomik.
///
/// <code>
/// searchengine-spbu                    ← alias yang dipakai mencari
///        └──► searchengine-spbu-20260912-013045   ← index fisik
/// </code>
/// </summary>
public static class SearchIndexNames
{
    private const string Spbu = "spbu";

    /// <summary>
    /// Alias yang dibaca saat pencarian.
    /// </summary>
    public static string SpbuAlias(
        string prefix)
    {
        return $"{prefix}-{Spbu}";
    }

    /// <summary>
    /// Pola pencocokan seluruh index fisik SPBU, dipakai untuk menemukan
    /// versi lama yang perlu dibersihkan.
    /// </summary>
    public static string SpbuPattern(
        string prefix)
    {
        return $"{prefix}-{Spbu}-*";
    }

    /// <summary>
    /// Nama index fisik baru. Cap waktu UTC dipakai agar urutannya jelas
    /// dan namanya tidak pernah bentrok.
    ///
    /// Formatnya sengaja hanya angka dan tanda hubung: Elasticsearch
    /// menolak nama index yang memuat huruf besar, sehingga pemisah seperti
    /// 'T' pada format ISO tidak dapat dipakai.
    /// </summary>
    public static string NewSpbuIndex(
        string prefix,
        DateTimeOffset waktu)
    {
        return $"{prefix}-{Spbu}-{waktu:yyyyMMdd-HHmmss}";
    }
}
