using SearchEngine.Application.Common.Models;

using MediatR;

namespace SearchEngine.Application.Features.Users.Commands.ToggleUserStatus;

public record ToggleUserStatusCommand(
    string Id)
    : IRequest<Result<string>>;
