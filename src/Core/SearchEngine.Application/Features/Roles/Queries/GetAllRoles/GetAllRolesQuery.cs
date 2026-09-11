using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.Roles.DTOs;
using MediatR;

namespace SearchEngine.Application.Features.Roles.Queries.GetAllRoles;

public record GetAllRolesQuery
    : IRequest<Result<List<ReadRoleResponse>>>;
