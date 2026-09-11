using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.Users.DTOs;
using MediatR;

namespace SearchEngine.Application.Features.Users.Queries.GetUserById;

public record GetUserByIdQuery(
    string Id)
    : IRequest<Result<ReadUserResponse>>;
