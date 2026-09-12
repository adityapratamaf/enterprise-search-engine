using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.Search.DTOs;

using MediatR;

namespace SearchEngine.Application.Features.Search.Queries.BenchmarkSearch;

public record BenchmarkSearchQuery(
    BenchmarkRequest Request)
    : IRequest<Result<BenchmarkResponse>>;
