using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.RolePermissions.DTOs;
using MediatR;

namespace SearchEngine.Application.Features.RolePermissions.Commands.UpdateModulePermission;

public record UpdateModulePermissionCommand(
    AssignModulePermissionRequest Request)
    : IRequest<Result<bool>>;
