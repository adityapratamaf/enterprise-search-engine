namespace SearchEngine.Application.Common.Search;

/// <summary>
/// Ringkasan satu kali proses indexing ulang penuh.
/// </summary>
/// <param name="IndexBaru">
/// Nama index fisik yang baru dibangun, mis. "searchengine-spbu-20260912-013045".
/// </param>
/// <param name="JumlahDokumen">Jumlah dokumen yang berhasil ditulis.</param>
/// <param name="JumlahGagal">
/// Jumlah dokumen yang ditolak Elasticsearch. Lebih dari nol berarti alias
/// TIDAK dipindahkan, sehingga index lama tetap melayani pencarian.
/// </param>
/// <param name="IndexLamaDihapus">
/// Index versi sebelumnya yang dibersihkan setelah alias berpindah.
/// </param>
/// <param name="Durasi">Lama proses berjalan.</param>
public sealed record SpbuReindexResult(
    string IndexBaru,
    int JumlahDokumen,
    int JumlahGagal,
    IReadOnlyCollection<string> IndexLamaDihapus,
    TimeSpan Durasi);
