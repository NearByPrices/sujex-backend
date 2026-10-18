using ErpCrm.Domain.Common;

namespace ErpCrm.Domain.Entities.Inventory;

public class Unit : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Symbol { get; set; }
    public bool IsActive { get; set; } = true;
}
