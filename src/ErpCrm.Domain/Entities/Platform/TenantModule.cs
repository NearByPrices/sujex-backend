namespace ErpCrm.Domain.Entities.Platform;

public class TenantModule : BaseEntity
{
    public Guid TenantId { get; set; }
    public required string ModuleCode { get; set; }
    public bool IsEnabled { get; set; } = true;
}
