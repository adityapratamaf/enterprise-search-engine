using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.Modules.DTOs;
using MediatR;

namespace SearchEngine.Application.Features.Modules.Queries.GetAllModules;

public record GetAllModulesQuery
    : IRequest<Result<List<ReadModuleResponse>>>;
