using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.Modules.DTOs;
using MediatR;

namespace SearchEngine.Application.Features.Modules.Commands.CreateModule;

public record CreateModuleCommand(
    CreateModuleRequest Request)
    : IRequest<Result<ReadModuleResponse>>;
