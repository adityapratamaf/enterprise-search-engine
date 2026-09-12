using SearchEngine.Application.Common.Search;

namespace SearchEngine.Application.Features.Search.DTOs;

/// <summary>
/// Hasil menjalankan kata kunci yang sama pada kedua mesin.
///
/// Jumlah hasil tetap disertakan bersama waktunya, karena mesin yang lebih
/// cepat tetapi tidak menemukan apa pun bukanlah mesin yang lebih baik.
/// </summary>
public sealed class BenchmarkResponse
{
    public string? Kueri { get; set; }

    /// <summary>Banyaknya pengukuran; waktu yang dilaporkan adalah mediannya.</summary>
    public int Iterasi { get; set; }

    /// <summary>Mesin yang unggul pada kueri ini.</summary>
    public SearchEngineKind Pemenang { get; set; }

    /// <summary>
    /// Berapa kali pemenang lebih cepat dibanding lawannya.
    /// </summary>
    public double KaliLebihCepat { get; set; }

    public BenchmarkEngineResult Elasticsearch { get; set; } = new();

    public BenchmarkEngineResult Sql { get; set; } = new();
}

public sealed class BenchmarkEngineResult
{
    public SearchEngineKind Engine { get; set; }

    /// <summary>Waktu pencarian dalam milidetik.</summary>
    public double WaktuMs { get; set; }

    public int TotalHasil { get; set; }

    /// <summary>Beberapa hasil teratas, untuk ditampilkan berdampingan.</summary>
    public List<SpbuSearchItem> Contoh { get; set; } = [];
}
