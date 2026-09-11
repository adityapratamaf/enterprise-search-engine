using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.PermissionActions.DTOs;
using MediatR;

namespace SearchEngine.Application.Features.PermissionActions.Queries.GetPermissionActionById;

public record GetPermissionActionByIdQuery(Guid Id)
    : IRequest<Result<PermissionActionResponse>>;
