using SearchEngine.Application.Features.Search.DTOs;

namespace SearchEngine.Application.Common.Interfaces;

/// <summary>
/// Saran ketik-langsung untuk kotak pencarian.
///
/// Hanya Elasticsearch yang mengimplementasikan ini. Saran yang layak pakai
/// menuntut pencocokan awalan pada setiap kata sekaligus peringkat
/// relevansi — dua hal yang tidak dapat diberikan <c>LIKE</c> pada basis
/// data dengan biaya yang masuk akal.
/// </summary>
public interface ISpbuSuggestionService
{
    Task<SpbuSuggestionResponse> SuggestAsync(
        string keyword,
        int limit,
        CancellationToken cancellationToken = default);
}
