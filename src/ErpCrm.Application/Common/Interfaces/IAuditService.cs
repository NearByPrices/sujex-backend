namespace ErpCrm.Application.Common.Interfaces;

public interface IAuditService
{
    Task LogAsync(string entityType, Guid entityId, string action, object? oldValues, object? newValues);
}
