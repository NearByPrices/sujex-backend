namespace ErpCrm.Application.Inventory.Dtos;

public record CreateProductDto(
    string Name,
    string? Code,
    string? Sku,
    string? Barcode,
    string? Description,
    Guid? CategoryId,
    Guid? UnitId,
    decimal CostPrice,
    decimal SalePrice,
    decimal? MinStockLevel,
    decimal? MaxStockLevel,
    decimal? ReorderPoint,
    bool IsService,
    string? Tags
);

public record UpdateProductDto(
    string Name,
    string? Code,
    string? Sku,
    string? Barcode,
    string? Description,
    Guid? CategoryId,
    Guid? UnitId,
    decimal CostPrice,
    decimal SalePrice,
    decimal? MinStockLevel,
    decimal? MaxStockLevel,
    decimal? ReorderPoint,
    bool IsActive,
    bool IsService,
    string? Tags
);

public record ProductResponseDto(
    Guid Id,
    string Name,
    string? Code,
    string? Sku,
    string? Barcode,
    string? Description,
    Guid? CategoryId,
    string? CategoryName,
    Guid? UnitId,
    string? UnitSymbol,
    decimal CostPrice,
    decimal SalePrice,
    bool IsActive,
    bool IsService,
    string? Tags,
    DateTime CreatedAt
);

public record StockLevelDto(
    Guid ProductId,
    string ProductName,
    Guid WarehouseId,
    string WarehouseName,
    decimal QuantityOnHand,
    decimal QuantityReserved,
    decimal QuantityAvailable
);
