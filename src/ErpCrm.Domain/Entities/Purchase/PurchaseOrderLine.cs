using ErpCrm.Domain.Common;

namespace ErpCrm.Domain.Entities.Purchase;

public class PurchaseOrderLine : TenantEntity
{
    public Guid PurchaseOrderId { get; set; }
    public Guid? ProductId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TaxPercent { get; set; }
    public decimal LineTotal => Quantity * UnitCost * (1 + TaxPercent / 100);
    public int SortOrder { get; set; }

    public PurchaseOrder PurchaseOrder { get; set; } = null!;
}
