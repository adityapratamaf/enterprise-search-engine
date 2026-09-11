using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.Search.DTOs;

using MediatR;

namespace SearchEngine.Application.Features.Search.Queries.SearchSpbu;

public record SearchSpbuQuery(
    SearchSpbuRequest Request)
    : IRequest<Result<SearchSpbuResponse>>;
