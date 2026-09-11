using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.PermissionActions.DTOs;
using MediatR;

namespace SearchEngine.Application.Features.PermissionActions.Commands.UpdatePermissionAction;

public record UpdatePermissionActionCommand(
    Guid Id,
    UpdatePermissionActionRequest Request)
    : IRequest<Result<PermissionActionResponse>>;
