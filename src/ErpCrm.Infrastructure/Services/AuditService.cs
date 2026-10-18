using ErpCrm.Application.Common.Interfaces;
using ErpCrm.Domain.Entities.Foundation;
using ErpCrm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace ErpCrm.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(AppDbContext db, ITenantContext tenantContext, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _tenantContext = tenantContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(string entityType, Guid entityId, string action, object? oldValues, object? newValues)
    {
        var log = new AuditLog
        {
            TenantId = _tenantContext.TenantId,
            UserId = _tenantContext.UserId,
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
            NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
            IPAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
            UserAgent = _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"]
        };

        _db.AuditLogs.Add(log);
        await _db.SaveChangesAsync();
    }
}
