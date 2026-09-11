using SearchEngine.Application.Common.Models;
using MediatR;

namespace SearchEngine.Application.Features.RolePermissions.Commands.RemoveModulePermission;

public record RemoveModulePermissionCommand(
    string RoleName,
    Guid ModuleId)
    : IRequest<Result<bool>>;
