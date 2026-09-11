using SearchEngine.Application.Common.Models;
using MediatR;

namespace SearchEngine.Application.Features.Roles.Commands.DeleteRole;

public record DeleteRoleCommand(
    string Id)
    : IRequest<Result<bool>>;
