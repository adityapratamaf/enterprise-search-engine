using SearchEngine.Application.Common.Models;
using SearchEngine.Application.Features.Audit.DTOs;
using MediatR;

namespace SearchEngine.Application.Features.Audit.Queries.GetAllAuditAction;

public record GetAllAuditActionQuery(
    PaginationRequest Request)
    : IRequest<
        Result<PaginatedResult<ReadAuditActionResponse>>>;
