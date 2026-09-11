namespace SearchEngine.Application.Common.Interfaces;

public interface IAuditActionService
{
    Task LogAsync(
        string action,
        string module,
        string tableName,
        string recordId,
        object? oldValues = null,
        object? newValues = null,
        CancellationToken cancellationToken = default);
}
