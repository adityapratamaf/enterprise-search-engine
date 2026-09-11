using SearchEngine.Application.Common.Search;

namespace SearchEngine.Application.Common.Interfaces;

/// <summary>
/// Menulis data SPBU dari basis data ke index pencarian.
///
/// Seluruh operasi sengaja menerima koleksi, bukan satu entitas. Bentuk ini
/// dipilih sejak awal supaya tahap berikutnya — ketika perubahan data
/// dialirkan lewat outbox dan jumlahnya jauh lebih besar — tidak perlu
/// mengubah kontrak: menulis satu dokumen cukup dikirim sebagai koleksi
/// berisi satu elemen, sedangkan menulis puluhan ribu tetap satu panggilan.
/// </summary>
public interface ISpbuIndexer
{
    /// <summary>
    /// Membangun ulang seluruh index dari awal, lalu memindahkan alias ke
    /// index baru secara atomik. Pencarian tidak pernah melihat index yang
    /// belum selesai dibangun, dan tidak ada jeda tanpa layanan.
    /// </summary>
    Task<SpbuReindexResult> ReindexAllAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Menulis atau memperbarui dokumen untuk SPBU tertentu pada index yang
    /// sedang aktif.
    /// </summary>
    Task<int> IndexAsync(
        IReadOnlyCollection<Guid> spbuIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Menghapus dokumen dari index yang sedang aktif.
    ///
    /// Wajib dipanggil ketika sebuah SPBU dihapus. Elasticsearch tidak tahu
    /// apa pun tentang basis data: dokumen yang tidak dihapus eksplisit akan
    /// terus muncul di hasil pencarian meski barisnya sudah tiada.
    /// </summary>
    Task<int> RemoveAsync(
        IReadOnlyCollection<Guid> spbuIds,
        CancellationToken cancellationToken = default);
}
