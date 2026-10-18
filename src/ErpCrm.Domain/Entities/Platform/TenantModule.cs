using ErpCrm.Domain.Common;
using ErpCrm.Domain.Enums;

namespace ErpCrm.Domain.Entities.Platform;

public class TenantModule : BaseEntity
{
    public Guid TenantId { get; set; }
    public ModuleCode ModuleCode { get; set; }
    public bool IsEnabled { get; set; } = true;

    public Tenant Tenant { get; set; } = null!;
}
