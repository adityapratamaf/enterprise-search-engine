using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.Search.DTOs;

using MediatR;

namespace SearchEngine.Application.Features.Search.Queries.SuggestSpbu;

public record SuggestSpbuQuery(
    string Keyword,
    int Limit)
    : IRequest<Result<SpbuSuggestionResponse>>;
