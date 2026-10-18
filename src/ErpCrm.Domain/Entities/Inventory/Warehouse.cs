using ErpCrm.Domain.Common;

namespace ErpCrm.Domain.Entities.Inventory;

public class Warehouse : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public Guid? BranchId { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
}
