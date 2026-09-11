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
}
