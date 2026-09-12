using SearchEngine.Application.Common.Search;
using SearchEngine.Application.Features.Search.DTOs;

namespace SearchEngine.Application.Common.Interfaces;

/// <summary>
/// Satu mesin yang mampu melayani pencarian SPBU.
///
/// Ada lebih dari satu implementasi yang terdaftar bersamaan; pemilihannya
/// dilakukan saat permintaan datang berdasarkan <see cref="Engine"/>.
/// Bentuk ini juga yang nanti dipakai endpoint pembanding untuk menjalankan
/// kueri yang sama pada kedua mesin.
/// </summary>
public interface ISpbuSearchProvider
{
    SearchEngineKind Engine { get; }

    Task<SearchSpbuResponse> SearchAsync(
        SearchSpbuRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Besar keseluruhan kumpulan data yang dilayani mesin ini, tanpa
    /// penyaring apa pun. Dipakai untuk menyatakan skala pengujian —
    /// "48 ms dari sejuta dokumen" bermakna lain dari "48 ms dari seribu".
    /// </summary>
    Task<long> HitungDokumenAsync(
        CancellationToken cancellationToken = default);
}
