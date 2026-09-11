using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.Auth.DTOs;

using MediatR;

namespace SearchEngine.Application.Features.Auth.Commands.ChangePassword;

public record ChangePasswordCommand(
    ChangePasswordRequest Request)
    : IRequest<Result<string>>;
