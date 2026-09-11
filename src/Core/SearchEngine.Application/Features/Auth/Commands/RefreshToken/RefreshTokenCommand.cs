using SearchEngine.Application.Common.Models;
using MediatR;

namespace SearchEngine.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommand : IRequest<Result<AuthResponse>>
{
    public string RefreshToken { get; set; }
        = string.Empty;
}
