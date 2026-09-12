namespace SearchEngine.Application.Features.Search.DTOs;

/// <summary>
/// Parameter pembandingan mesin pencari.
/// </summary>
public sealed class BenchmarkRequest
{
    /// <summary>
    /// Kata kunci yang diuji. Boleh berupa kata yang belum selesai diketik
    /// — justru di situ selisih kemampuan kedua mesin paling terlihat.
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// Banyaknya pengukuran per mesin. Dibuat kecil karena satu kueri SQL
    /// pada data besar dapat memakan beberapa detik, dan permintaan HTTP
    /// tidak sepantasnya menggantung lama.
    /// </summary>
    public int Iterasi { get; set; } = 3;

    /// <summary>
    /// Menjalankan sekali tanpa dihitung sebelum pengukuran dimulai.
    /// Eksekusi pertama selalu menanggung kompilasi kueri dan pemanasan
    /// koneksi, sehingga memasukkannya membuat perbandingan tidak adil.
    /// </summary>
    public bool Warmup { get; set; } = true;

    /// <summary>
    /// Banyaknya hasil teratas yang disertakan dari tiap mesin untuk
    /// ditampilkan berdampingan.
    /// </summary>
    public int Contoh { get; set; } = 3;
}
