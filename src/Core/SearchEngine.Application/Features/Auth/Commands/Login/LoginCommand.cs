using SearchEngine.Application.Common.Models;
using MediatR;

namespace SearchEngine.Application.Features.Auth.Commands.Login;

public class LoginCommand : IRequest<Result<AuthResponse>>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
