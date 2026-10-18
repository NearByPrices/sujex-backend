namespace ErpCrm.Domain.Entities.Platform;

public class TenantModule : TenantEntity
{
    public required string ModuleCode { get; set; }
    public bool IsEnabled { get; set; } = true;
}
