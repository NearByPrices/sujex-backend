namespace ErpCrm.Application.Inventory.Dtos;

public record CreateStockMovementDto(
    Guid ProductId,
    Guid WarehouseId,
    string MovementType,
    decimal Quantity,
    decimal UnitCost,
    string? Reference,
    string? Notes,
    DateTime? MovementDate
);

public record StockMovementResponseDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    Guid WarehouseId,
    string WarehouseName,
    string MovementType,
    decimal Quantity,
    decimal UnitCost,
    decimal TotalCost,
    string? Reference,
    string? Notes,
    DateTime MovementDate,
    DateTime CreatedAt
);
