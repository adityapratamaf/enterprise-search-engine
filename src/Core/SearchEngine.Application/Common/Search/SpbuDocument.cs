using System.Text.Json.Serialization;

namespace SearchEngine.Application.Common.Search;

/// <summary>
/// Bentuk dokumen SPBU di dalam Elasticsearch.
///
/// Ini adalah proyeksi yang sengaja didenormalisasi: rantai
/// SPBU → Kota → Provinsi → Regional serta relasi produk dan fasilitas
/// diratakan menjadi field datar. Akibatnya pencarian tidak perlu melakukan
/// join sama sekali — seluruh informasi yang dibutuhkan untuk menyaring,
/// mengurutkan, dan menampilkan hasil sudah ada di satu dokumen.
///
/// Nama field dikunci lewat <see cref="JsonPropertyNameAttribute"/> agar
/// selalu sama dengan mapping index, tidak bergantung pada konvensi
/// serialisasi yang bisa berubah.
/// </summary>
public sealed class SpbuDocument
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("kodeSpbu")]
    public string KodeSpbu { get; set; } = default!;

    [JsonPropertyName("nama")]
    public string Nama { get; set; } = default!;

    [JsonPropertyName("alamat")]
    public string Alamat { get; set; } = default!;

    [JsonPropertyName("kodePos")]
    public string? KodePos { get; set; }

    // ---- Wilayah (diratakan dari rantai induk) ----

    [JsonPropertyName("kota")]
    public string Kota { get; set; } = default!;

    [JsonPropertyName("kodeKota")]
    public string KodeKota { get; set; } = default!;

    [JsonPropertyName("provinsi")]
    public string Provinsi { get; set; } = default!;

    [JsonPropertyName("kodeProvinsi")]
    public string KodeProvinsi { get; set; } = default!;

    // ---- Regional Pertamina (diturunkan lewat provinsi) ----

    [JsonPropertyName("regional")]
    public string Regional { get; set; } = default!;

    [JsonPropertyName("regionalNama")]
    public string RegionalNama { get; set; } = default!;

    [JsonPropertyName("regionalNomor")]
    public int RegionalNomor { get; set; }

    // ---- Atribut SPBU ----

    [JsonPropertyName("tipeKepemilikan")]
    public string TipeKepemilikan { get; set; } = default!;

    [JsonPropertyName("status")]
    public string Status { get; set; } = default!;

    [JsonPropertyName("jumlahDispenser")]
    public int JumlahDispenser { get; set; }

    [JsonPropertyName("jumlahNozzle")]
    public int JumlahNozzle { get; set; }

    [JsonPropertyName("tanggalOperasi")]
    public DateTime? TanggalOperasi { get; set; }

    [JsonPropertyName("nomorTelepon")]
    public string? NomorTelepon { get; set; }

    /// <summary>
    /// Rata-rata penilaian, 1,0-5,0. <c>null</c> bila belum ada ulasan —
    /// berbeda artinya dari nol.
    /// </summary>
    [JsonPropertyName("rating")]
    public decimal? Rating { get; set; }

    [JsonPropertyName("jumlahUlasan")]
    public int JumlahUlasan { get; set; }

    [JsonPropertyName("lokasi")]
    public SpbuLokasi Lokasi { get; set; } = new();

    // ---- Relasi (diratakan menjadi daftar nilai) ----

    [JsonPropertyName("produk")]
    public string[] Produk { get; set; } = [];

    [JsonPropertyName("produkNama")]
    public string[] ProdukNama { get; set; } = [];

    [JsonPropertyName("jenisBbm")]
    public string[] JenisBbm { get; set; } = [];

    [JsonPropertyName("fasilitas")]
    public string[] Fasilitas { get; set; } = [];

    [JsonPropertyName("fasilitasNama")]
    public string[] FasilitasNama { get; set; } = [];
}

/// <summary>
/// Titik koordinat dalam bentuk yang diterima tipe <c>geo_point</c>
/// Elasticsearch.
/// </summary>
public sealed class SpbuLokasi
{
    [JsonPropertyName("lat")]
    public double Lat { get; set; }

    [JsonPropertyName("lon")]
    public double Lon { get; set; }
}
