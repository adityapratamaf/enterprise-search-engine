namespace SearchEngine.Application.Features.Search.DTOs;

/// <summary>
/// Parameter pembandingan mesin pencari.
///
/// Penyaring dan paginasinya sengaja ditulis ulang di sini alih-alih
/// mewarisi <see cref="SearchSpbuRequest"/>, supaya properti yang tidak
/// bermakna pada pembandingan — pemilihan mesin dan facet — tidak ikut
/// muncul sebagai parameter yang membingungkan pembaca dokumentasi.
/// </summary>
public sealed class BenchmarkRequest
{
    /// <summary>
    /// Kata kunci yang diuji. Boleh berupa kata yang belum selesai diketik
    /// atau salah ketik — justru di situ selisih kemampuan kedua mesin
    /// paling terlihat.
    /// </summary>
    public string? Search { get; set; }

    // ---- Paginasi ----

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Kolom pengurutan. Kosong berarti relevansi pada Elasticsearch;
    /// SQL selalu jatuh ke pengurutan abjad karena tidak punya skor.
    /// </summary>
    public string? SortBy { get; set; }

    public bool IsDescending { get; set; }

    // ---- Penyaring ----

    public string[]? Regional { get; set; }

    public string[]? Provinsi { get; set; }

    public string[]? Kota { get; set; }

    public string[]? Produk { get; set; }

    public string[]? Fasilitas { get; set; }

    public string[]? Status { get; set; }

    public string[]? TipeKepemilikan { get; set; }

    public double? RatingMin { get; set; }

    public int? UlasanMin { get; set; }

    // ---- Penyaring jarak ----

    public double? Lat { get; set; }

    public double? Lon { get; set; }

    public double? RadiusKm { get; set; }

    public double? LatMin { get; set; }

    public double? LonMin { get; set; }

    public double? LatMax { get; set; }

    public double? LonMax { get; set; }

    // ---- Pengukuran ----

    /// <summary>
    /// Banyaknya pengukuran per mesin. Dibuat kecil karena satu kueri SQL
    /// pada data besar dapat memakan beberapa detik, dan permintaan HTTP
    /// tidak sepantasnya menggantung lama.
    ///
    /// Isi 1 saat pengguna sekadar berpindah halaman: angka waktunya tetap
    /// benar, tanpa menunggu pengukuran berulang.
    /// </summary>
    public int Iterasi { get; set; } = 3;

    /// <summary>
    /// Menjalankan sekali tanpa dihitung sebelum pengukuran dimulai.
    /// Eksekusi pertama selalu menanggung kompilasi kueri dan pemanasan
    /// koneksi, sehingga memasukkannya membuat perbandingan tidak adil.
    /// </summary>
    public bool Warmup { get; set; } = true;
}
