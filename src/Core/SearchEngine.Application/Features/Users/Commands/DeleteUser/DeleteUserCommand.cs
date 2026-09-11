using SearchEngine.Application.Common.Models;

using MediatR;

namespace SearchEngine.Application.Features.Users.Commands.DeleteUser;

public record DeleteUserCommand(
    string Id)
    : IRequest<Result<string>>;
