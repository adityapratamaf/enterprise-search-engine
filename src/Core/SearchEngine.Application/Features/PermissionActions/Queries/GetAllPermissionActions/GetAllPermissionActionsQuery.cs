using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.PermissionActions.DTOs;
using MediatR;

namespace SearchEngine.Application.Features.PermissionActions.Queries.GetAllPermissionActions;

public record GetAllPermissionActionsQuery
    : IRequest<Result<List<PermissionActionResponse>>>;
