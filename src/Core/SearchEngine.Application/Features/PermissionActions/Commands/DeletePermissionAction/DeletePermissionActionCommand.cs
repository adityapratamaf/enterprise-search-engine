using SearchEngine.Application.Common.Models;
using MediatR;

namespace SearchEngine.Application.Features.PermissionActions.Commands.DeletePermissionAction;

public record DeletePermissionActionCommand(Guid Id)
    : IRequest<Result<bool>>;
