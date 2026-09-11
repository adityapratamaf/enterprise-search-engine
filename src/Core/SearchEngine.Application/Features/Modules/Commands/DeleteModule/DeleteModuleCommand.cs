using SearchEngine.Application.Common.Models;
using MediatR;

namespace SearchEngine.Application.Features.Modules.Commands.DeleteModule;

public record DeleteModuleCommand(
    Guid Id)
    : IRequest<Result<bool>>;
