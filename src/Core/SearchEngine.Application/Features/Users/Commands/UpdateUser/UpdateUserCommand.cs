using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.Users.DTOs;

using MediatR;

namespace SearchEngine.Application.Features.Users.Commands.UpdateUser;

public record UpdateUserCommand(
    string Id,
    UpdateUserRequest Request)
    : IRequest<Result<ReadUserResponse>>;
