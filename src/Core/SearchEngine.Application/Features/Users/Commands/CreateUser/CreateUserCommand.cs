using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.Users.DTOs;
using MediatR;

namespace SearchEngine.Application.Features.Users.Commands.CreateUser;

public record CreateUserCommand(
    CreateUserRequest Request)
    : IRequest<Result<ReadUserResponse>>;
