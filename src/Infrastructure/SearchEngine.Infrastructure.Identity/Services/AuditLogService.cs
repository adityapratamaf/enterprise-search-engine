using System.Text.Json;
using SearchEngine.Application.Common.Interfaces;
using SearchEngine.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace SearchEngine.Infrastructure.Identity.Services;

public class AuditLogService
    : IAuditActionService
{
    private readonly IApplicationIdentityDbContext
        _context;

    private readonly IHttpContextAccessor
        _httpContextAccessor;

    // Claims
    private readonly ICurrentUserService
        _currentUser;

    public AuditLogService(
        IApplicationIdentityDbContext context,
        IHttpContextAccessor httpContextAccessor,
        ICurrentUserService currentUser)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _currentUser = currentUser;
    }

    public async Task LogAsync(
        string action,
        string module,
        string tableName,
        string recordId,
        object? oldValues = null,
        object? newValues = null,
        CancellationToken cancellationToken = default)
    {
        var httpContext =
            _httpContextAccessor.HttpContext;

        var log = new AuditAction
        {
            Id = Guid.NewGuid(),
            UserEmail = _currentUser.Email,
            Action = action,
            Module = module,
            TableName = tableName,
            RecordId = recordId,

            OldValues = oldValues is null
                ? null
                : JsonSerializer.Serialize(
                    oldValues),

            NewValues = newValues is null
                ? null
                : JsonSerializer.Serialize(
                    newValues),

            IpAddress =
                httpContext?
                    .Connection
                    .RemoteIpAddress?
                    .ToString(),

            UserAgent =
                httpContext?
                    .Request
                    .Headers["User-Agent"],

            CreatedAt = DateTime.UtcNow
        };

        await _context.AuditLogs
            .AddAsync(log);

        await _context.SaveChangesAsync(
            cancellationToken);
    }
}
