using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.Roles.DTOs;
using MediatR;

namespace SearchEngine.Application.Features.Roles.Queries.GetRoleById;

public record GetRoleByIdQuery(
    string Id)
    : IRequest<Result<ReadRoleResponse>>;
