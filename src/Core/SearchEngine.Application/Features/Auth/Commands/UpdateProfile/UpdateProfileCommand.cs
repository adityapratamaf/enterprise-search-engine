using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.Auth.DTOs;
using SearchEngine.Application.Features.Users.DTOs;

using MediatR;

namespace SearchEngine.Application.Features.Auth.Commands.UpdateProfile;

public record UpdateProfileCommand(
    UpdateProfileRequest Request)
    : IRequest<Result<ReadUserResponse>>;
