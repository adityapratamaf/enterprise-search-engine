using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.PermissionActions.DTOs;
using MediatR;

namespace SearchEngine.Application.Features.PermissionActions.Commands.CreatePermissionAction;

public record CreatePermissionActionCommand(
    CreatePermissionActionRequest Request)
    : IRequest<Result<PermissionActionResponse>>;
