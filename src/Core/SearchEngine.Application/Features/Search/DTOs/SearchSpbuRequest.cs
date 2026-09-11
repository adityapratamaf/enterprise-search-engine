using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Common.Search;

namespace SearchEngine.Application.Features.Search.DTOs;

/// <summary>
/// Parameter pencarian SPBU.
///
/// Mewarisi <see cref="PaginationRequest"/> sehingga <c>PageNumber</c>,
/// <c>PageSize</c>, <c>Search</c>, <c>SortBy</c>, dan <c>IsDescending</c>
/// mengikuti konvensi yang sudah dipakai endpoint lain di aplikasi ini.
/// <c>Search</c> berperan sebagai kata kunci utama.
///
/// Seluruh penyaring berbentuk larik agar mendukung pemilihan berganda pada
/// panel facet — nilainya sengaja sama persis dengan yang dikembalikan facet,
/// sehingga klien cukup meneruskan nilai yang diklik pengguna tanpa
/// penerjemahan apa pun.
/// </summary>
public class SearchSpbuRequest
    : PaginationRequest
{
    /// <summary>
    /// Mesin pencari yang dipakai. Bawaan: Elasticsearch.
    /// </summary>
    public SearchEngineKind Engine { get; set; } =
        SearchEngineKind.Elasticsearch;

    /// <summary>Kode regional Pertamina, mis. "JBB".</summary>
    public string[]? Regional { get; set; }

    /// <summary>Nama provinsi, mis. "Jawa Barat".</summary>
    public string[]? Provinsi { get; set; }

    /// <summary>Nama kota/kabupaten, mis. "Kota Semarang".</summary>
    public string[]? Kota { get; set; }

    /// <summary>Kode produk, mis. "PERTALITE".</summary>
    public string[]? Produk { get; set; }

    /// <summary>Kode fasilitas, mis. "ATM".</summary>
    public string[]? Fasilitas { get; set; }

    /// <summary>Status operasional, mis. "Aktif".</summary>
    public string[]? Status { get; set; }

    /// <summary>Pola kepemilikan, mis. "Dodo".</summary>
    public string[]? TipeKepemilikan { get; set; }

    // ---- Penyaring jarak ----

    public double? Lat { get; set; }

    public double? Lon { get; set; }

    /// <summary>
    /// Radius pencarian dalam kilometer, dihitung dari
    /// <see cref="Lat"/> dan <see cref="Lon"/>.
    /// </summary>
    public double? RadiusKm { get; set; }

    /// <summary>
    /// Menyertakan hitungan facet pada hasil. Dapat dimatikan bila klien
    /// hanya membutuhkan daftar hasil, mis. saat berpindah halaman.
    /// </summary>
    public bool IncludeFacets { get; set; } = true;
}
