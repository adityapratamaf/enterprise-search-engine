using SearchEngine.Application.Common.Models;
using MediatR;

namespace SearchEngine.Application.Features.Auth.Commands.Logout;

public record LogoutCommand
    : IRequest<Result<string>>;
