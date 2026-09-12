using SearchEngine.Application.Common.Search;

namespace SearchEngine.Application.Features.Search.DTOs;

/// <summary>
/// Hasil pencarian SPBU beserta keterangan tentang bagaimana ia diperoleh.
/// </summary>
public sealed class SearchSpbuResponse
{
    public List<SpbuSearchItem> Items { get; set; } = [];

    public int TotalCount { get; set; }

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalPages { get; set; }

    /// <summary>
    /// Lama pencarian dieksekusi pada mesinnya, dalam milidetik. Inilah
    /// angka yang ditampilkan sebagai "sekian hasil (0,03 detik)".
    /// </summary>
    public long TookMs { get; set; }

    public SearchEngineKind Engine { get; set; }

    /// <summary>
    /// Hitungan per nilai untuk panel penyaring. <c>null</c> bila mesin yang
    /// dipakai tidak mampu menghasilkannya — bukan berarti tidak ada hasil.
    /// Kunci: regional, provinsi, kota, produk, fasilitas, status,
    /// tipeKepemilikan.
    /// </summary>
    public Dictionary<string, List<FacetBucket>>? Facets { get; set; }

    /// <summary>
    /// Kemampuan yang benar-benar aktif pada permintaan ini. Klien memakai
    /// ini untuk membedakan "mesin tidak bisa melakukannya" dari "tidak ada
    /// hasil", sehingga tampilan tidak diam-diam kosong tanpa penjelasan.
    /// </summary>
    public SearchCapabilities Kemampuan { get; set; } = new();

    /// <summary>
    /// Keterangan mengenai permintaan yang tidak dapat dipenuhi sepenuhnya,
    /// mis. penyaring jarak yang diabaikan karena mesinnya tidak mendukung.
    /// </summary>
    public List<string> Catatan { get; set; } = [];
}

/// <summary>
/// Satu SPBU pada hasil pencarian. Memuat dokumen utuh sehingga klien dapat
/// menampilkan halaman rinci tanpa permintaan tambahan.
/// </summary>
public sealed class SpbuSearchItem
{
    public Guid Id { get; set; }

    public string KodeSpbu { get; set; } = default!;

    public string Nama { get; set; } = default!;

    public string Alamat { get; set; } = default!;

    public string? KodePos { get; set; }

    public string Kota { get; set; } = default!;

    public string Provinsi { get; set; } = default!;

    public string Regional { get; set; } = default!;

    public string RegionalNama { get; set; } = default!;

    public string TipeKepemilikan { get; set; } = default!;

    public string Status { get; set; } = default!;

    public int JumlahDispenser { get; set; }

    public int JumlahNozzle { get; set; }

    public DateTime? TanggalOperasi { get; set; }

    public string? NomorTelepon { get; set; }

    /// <summary>Rata-rata penilaian; <c>null</c> bila belum ada ulasan.</summary>
    public decimal? Rating { get; set; }

    public int JumlahUlasan { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public string[] Produk { get; set; } = [];

    public string[] ProdukNama { get; set; } = [];

    public string[] Fasilitas { get; set; } = [];

    public string[] FasilitasNama { get; set; } = [];

    /// <summary>
    /// Skor relevansi. <c>null</c> bila mesin tidak memeringkat hasil.
    /// </summary>
    public double? Score { get; set; }

    /// <summary>
    /// Potongan teks dengan bagian yang cocok ditandai
    /// <c>&lt;mark&gt;</c>. Kunci adalah nama field (nama, alamat).
    /// <c>null</c> bila mesin tidak mendukung penyorotan.
    /// </summary>
    public Dictionary<string, string[]>? Highlight { get; set; }

    /// <summary>
    /// Jarak dari titik acuan, diisi hanya bila pencarian menyertakan
    /// koordinat.
    /// </summary>
    public double? JarakKm { get; set; }
}

public sealed class FacetBucket
{
    public string Nilai { get; set; } = default!;

    public long Jumlah { get; set; }
}

/// <summary>
/// Penanda kemampuan mesin pencari pada permintaan yang bersangkutan.
/// </summary>
public sealed class SearchCapabilities
{
    /// <summary>Penyorotan kata yang cocok pada hasil.</summary>
    public bool Highlight { get; set; }

    /// <summary>Hitungan facet untuk panel penyaring.</summary>
    public bool Facet { get; set; }

    /// <summary>Toleransi salah ketik.</summary>
    public bool Fuzzy { get; set; }

    /// <summary>Pengurutan berdasarkan relevansi, bukan sekadar abjad.</summary>
    public bool Relevansi { get; set; }

    /// <summary>Sinonim alamat, mis. "jl" dikenali sebagai "jalan".</summary>
    public bool Sinonim { get; set; }

    /// <summary>Penyaringan dan pengurutan berdasarkan jarak.</summary>
    public bool Geo { get; set; }
}
