using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.Search.DTOs;

using MediatR;

namespace SearchEngine.Application.Features.Search.Queries.SuggestSpbu;

public class SuggestSpbuHandler
    : IRequestHandler<
        SuggestSpbuQuery,
        Result<SpbuSuggestionResponse>>
{
    private const int LimitMaksimum = 20;

    private readonly ISpbuSuggestionService _suggestion;

    public SuggestSpbuHandler(
        ISpbuSuggestionService suggestion)
    {
        _suggestion = suggestion;
    }

    public async Task<Result<SpbuSuggestionResponse>> Handle(
        SuggestSpbuQuery request,
        CancellationToken cancellationToken)
    {
        var keyword =
            request.Keyword?.Trim() ?? string.Empty;

        // Saran untuk satu huruf hampir tidak bermakna dan memaksa
        // Elasticsearch menelusuri hampir seluruh index. Dikembalikan kosong
        // saja, bukan error, supaya klien tidak perlu menjaga ambang batas.
        if (keyword.Length < 2)
        {
            return Result<SpbuSuggestionResponse>
                .SuccessResult(
                    new SpbuSuggestionResponse(),
                    "Kata kunci terlalu pendek.");
        }

        var limit =
            Math.Clamp(request.Limit, 1, LimitMaksimum);

        var hasil =
            await _suggestion.SuggestAsync(
                keyword,
                limit,
                cancellationToken);

        return Result<SpbuSuggestionResponse>
            .SuccessResult(hasil);
    }
}
