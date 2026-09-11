using System.ComponentModel.DataAnnotations;

namespace SearchEngine.Infrastructure.Search;

/// <summary>
/// Konfigurasi sambungan dan perilaku indexing Elasticsearch, diikat dari
/// bagian "Elasticsearch" pada appsettings.
/// </summary>
public sealed class ElasticsearchOptions
{
    public const string SectionName = "Elasticsearch";

    [Required]
    public string Uri { get; set; } = default!;

    [Required]
    public string Username { get; set; } = default!;

    [Required]
    public string Password { get; set; } = default!;

    /// <summary>
    /// Awalan seluruh nama index milik aplikasi. Nilainya juga menentukan
    /// batas hak akses user Elasticsearch yang dipakai backend.
    /// </summary>
    [Required]
    public string IndexPrefix { get; set; } = "searchengine";

    [Range(1, 300)]
    public int RequestTimeoutSeconds { get; set; } = 30;

    [Range(0, 10)]
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Menyimpan permintaan dan tanggapan mentah pada objek respons.
    /// Berguna saat menelusuri masalah, tetapi menambah pemakaian memori —
    /// biarkan mati di luar pengembangan.
    /// </summary>
    public bool EnableDebugMode { get; set; }

    /// <summary>
    /// Jumlah dokumen per permintaan bulk. Terlalu kecil membuat proses
    /// bolak-balik jaringan, terlalu besar membebani memori node.
    /// </summary>
    [Range(100, 10_000)]
    public int BulkBatchSize { get; set; } = 2_000;

    [Range(1, 100)]
    public int DefaultShards { get; set; } = 1;

    [Range(0, 10)]
    public int DefaultReplicas { get; set; }
}
