using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.RolePermissions.DTOs;
using MediatR;

namespace SearchEngine.Application.Features.RolePermissions.Commands.AssignModulePermission;

public record AssignModulePermissionCommand(
    AssignModulePermissionRequest Request)
    : IRequest<Result<bool>>;
