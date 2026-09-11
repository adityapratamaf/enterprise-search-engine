using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.Auth.DTOs;
using MediatR;

namespace SearchEngine.Application.Features.Auth.Queries.GetCurrentUser;

public record GetCurrentUserQuery
    : IRequest<Result<CurrentUserAuthResponse>>;
