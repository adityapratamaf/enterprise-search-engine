using SearchEngine.Application.Common.Exceptions;
using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.Search.DTOs;

using MediatR;

namespace SearchEngine.Application.Features.Search.Queries.SearchSpbu;

public class SearchSpbuHandler
    : IRequestHandler<
        SearchSpbuQuery,
        Result<SearchSpbuResponse>>
{
    private readonly IEnumerable<ISpbuSearchProvider> _providers;

    public SearchSpbuHandler(
        IEnumerable<ISpbuSearchProvider> providers)
    {
        _providers = providers;
    }

    public async Task<Result<SearchSpbuResponse>> Handle(
        SearchSpbuQuery request,
        CancellationToken cancellationToken)
    {
        var provider =
            _providers.FirstOrDefault(x =>
                x.Engine == request.Request.Engine)
            ?? throw new NotFoundException(
                $"Mesin pencari '{request.Request.Engine}' tidak tersedia.");

        var hasil =
            await provider.SearchAsync(
                request.Request,
                cancellationToken);

        var pesan =
            $"{hasil.TotalCount:N0} hasil dalam {hasil.TookMs} ms "
            + $"({hasil.Engine}).";

        return Result<SearchSpbuResponse>
            .SuccessResult(hasil, pesan);
    }
}
