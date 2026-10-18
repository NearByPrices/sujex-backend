using ErpCrm.Domain.Enums;

namespace ErpCrm.Application.Sales.Dtos;

public record SalesOrderLineDto(
    Guid? ProductId,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountPercent,
    decimal TaxPercent,
    int SortOrder
);

public record CreateSalesOrderDto(
    Guid? ContactId,
    Guid? CompanyId,
    DateTime OrderDate,
    DateTime? DeliveryDate,
    string? BillingAddress,
    string? ShippingAddress,
    string? CurrencyCode,
    decimal ExchangeRate,
    string? Notes,
    Guid? AssignedToUserId,
    List<SalesOrderLineDto> Lines
);

public record UpdateSalesOrderDto(
    Guid? ContactId,
    Guid? CompanyId,
    DateTime OrderDate,
    DateTime? DeliveryDate,
    string? BillingAddress,
    string? ShippingAddress,
    string? CurrencyCode,
    decimal ExchangeRate,
    string? Notes,
    Guid? AssignedToUserId,
    List<SalesOrderLineDto> Lines
);

public record SalesOrderResponseDto(
    Guid Id,
    string OrderNumber,
    Guid? ContactId,
    Guid? CompanyId,
    SalesOrderStatus Status,
    DateTime OrderDate,
    DateTime? DeliveryDate,
    decimal SubTotal,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal TotalAmount,
    string? Notes,
    List<SalesOrderLineResponseDto> Lines,
    DateTime CreatedAt
);

public record SalesOrderLineResponseDto(
    Guid Id,
    Guid? ProductId,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountPercent,
    decimal TaxPercent,
    decimal LineTotal,
    int SortOrder
);
