using ErpCrm.Domain.Common;

namespace ErpCrm.Domain.Entities.Foundation;

public class AuditLog : TenantEntity
{
    public Guid? CompanyId { get; set; }
    public Guid UserId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Action { get; set; } = string.Empty; // Create/Update/Delete/Post
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IPAddress { get; set; }
    public string? UserAgent { get; set; }
}
