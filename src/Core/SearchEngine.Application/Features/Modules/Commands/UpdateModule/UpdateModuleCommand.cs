using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.Modules.DTOs;
using MediatR;

namespace SearchEngine.Application.Features.Modules.Commands.UpdateModule;

public record UpdateModuleCommand(
    Guid Id,
    UpdateModuleRequest Request)
    : IRequest<Result<bool>>;
