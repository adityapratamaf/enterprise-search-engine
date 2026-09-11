namespace SearchEngine.Application.Features.Search.DTOs;

/// <summary>
/// Daftar saran yang ditampilkan saat pengguna masih mengetik.
/// </summary>
public sealed class SpbuSuggestionResponse
{
    public List<SpbuSuggestionItem> Items { get; set; } = [];

    public long TookMs { get; set; }
}

/// <summary>
/// Satu baris saran. Menyertakan kode dan wilayah agar klien dapat
/// menampilkan konteks — "SPBU Jenderal Sudirman · Jakarta Selatan" — dan
/// langsung membuka SPBU-nya ketika saran dipilih, tanpa perlu mencari lagi.
/// </summary>
public sealed class SpbuSuggestionItem
{
    public string KodeSpbu { get; set; } = default!;

    public string Nama { get; set; } = default!;

    public string Kota { get; set; } = default!;

    public string Provinsi { get; set; } = default!;
}
