using SearchEngine.Application.Common.Interfaces;

using Hangfire;

namespace SearchEngine.WebAPI.Jobs;

/// <summary>
/// Pembungkus Hangfire untuk indexing ulang penuh.
///
/// Kelas ini ada karena dua alasan yang tidak bisa dipenuhi dengan
/// mengantrikan <see cref="ISpbuIndexer"/> secara langsung:
///
/// 1. <see cref="DisableConcurrentExecutionAttribute"/> mencegah dua proses
///    indexing ulang berjalan bersamaan. Tanpa itu, dua proses dapat
///    berebut memindahkan alias dan salah satunya berakhir menunjuk index
///    yang sudah dihapus proses lainnya.
///
/// 2. Hangfire mengantrikan metode yang mengembalikan <see cref="Task"/>,
///    sedangkan indexer mengembalikan ringkasan hasil.
/// </summary>
public sealed class SpbuReindexJob
{
    private readonly ISpbuIndexer _indexer;

    private readonly ILogger<SpbuReindexJob> _logger;

    public SpbuReindexJob(
        ISpbuIndexer indexer,
        ILogger<SpbuReindexJob> logger)
    {
        _indexer = indexer;
        _logger = logger;
    }

    [DisableConcurrentExecution(
        timeoutInSeconds: 3600)]
    [AutomaticRetry(Attempts = 2)]
    public async Task RunAsync(
        CancellationToken cancellationToken)
    {
        var hasil =
            await _indexer.ReindexAllAsync(cancellationToken);

        if (hasil.JumlahGagal > 0)
        {
            // Dilempar agar job ditandai gagal di dasbor Hangfire — proses
            // yang menyisakan dokumen ditolak tidak boleh terlihat sukses.
            throw new InvalidOperationException(
                $"Indexing ulang menyisakan {hasil.JumlahGagal} dokumen "
                + $"yang ditolak. Alias tidak dipindahkan; periksa index "
                + $"{hasil.IndexBaru}.");
        }

        _logger.LogInformation(
            "Indexing ulang SPBU selesai: {Jumlah} dokumen dalam {Durasi:0.0}s.",
            hasil.JumlahDokumen,
            hasil.Durasi.TotalSeconds);
    }
}
