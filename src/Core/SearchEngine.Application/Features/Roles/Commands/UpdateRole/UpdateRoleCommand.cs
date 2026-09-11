using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.Roles.DTOs;
using MediatR;

namespace SearchEngine.Application.Features.Roles.Commands.UpdateRole;

public record UpdateRoleCommand(
    string Id,
    UpdateRoleRequest Request)
    : IRequest<Result<bool>>;
