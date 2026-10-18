using ErpCrm.Domain.Common;

namespace ErpCrm.Domain.Entities.Inventory;

public class Product : SoftDeleteEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Sku { get; set; }
    public string? Barcode { get; set; }
    public string? Description { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? UnitId { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SalePrice { get; set; }
    public decimal? MinStockLevel { get; set; }
    public decimal? MaxStockLevel { get; set; }
    public decimal? ReorderPoint { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsService { get; set; } = false;
    public string? ImageUrl { get; set; }
    public string? Tags { get; set; }

    public ProductCategory? Category { get; set; }
    public Unit? Unit { get; set; }
    public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
}
