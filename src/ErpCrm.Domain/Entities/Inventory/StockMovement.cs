using ErpCrm.Domain.Common;

namespace ErpCrm.Domain.Entities.Inventory;

public class StockMovement : TenantEntity
{
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public string MovementType { get; set; } = string.Empty; // IN, OUT, TRANSFER, ADJUSTMENT
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost => Quantity * UnitCost;
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public DateTime MovementDate { get; set; } = DateTime.UtcNow;
    public Guid? RelatedOrderId { get; set; }
    public string? RelatedOrderType { get; set; }

    public Product Product { get; set; } = null!;
    public Warehouse Warehouse { get; set; } = null!;
}
