using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.Users.DTOs;

using MediatR;

namespace SearchEngine.Application.Features.Users.Queries.GetAllUsers;

public record GetAllUsersQuery(
    PaginationRequest Request)
    : IRequest<
        Result<
            PaginatedResult<ReadUserResponse>>>;
