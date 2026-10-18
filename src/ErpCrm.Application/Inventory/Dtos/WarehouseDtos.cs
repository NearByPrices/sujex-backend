namespace ErpCrm.Application.Inventory.Dtos;

public record CreateWarehouseDto(
    string Name,
    string? Code,
    Guid? BranchId,
    string? Address
);

public record UpdateWarehouseDto(
    string Name,
    string? Code,
    Guid? BranchId,
    string? Address,
    bool IsActive
);

public record WarehouseResponseDto(
    Guid Id,
    string Name,
    string? Code,
    Guid? BranchId,
    string? Address,
    bool IsActive,
    DateTime CreatedAt
);
