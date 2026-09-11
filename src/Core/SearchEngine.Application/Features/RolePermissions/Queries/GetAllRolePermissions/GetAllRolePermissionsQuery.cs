using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.RolePermissions.DTOs;
using MediatR;

namespace SearchEngine.Application.Features.RolePermissions.Queries.GetAllRolePermissions;

public record GetAllRolePermissionsQuery(
    string RoleName)
    : IRequest<Result<List<ReadRolePermissionResponse>>>;
