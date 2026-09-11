using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.Roles.DTOs;
using MediatR;

namespace SearchEngine.Application.Features.Roles.Commands.CreateRole;

public record CreateRoleCommand(
    CreateRoleRequest Request)
    : IRequest<Result<ReadRoleResponse>>;
