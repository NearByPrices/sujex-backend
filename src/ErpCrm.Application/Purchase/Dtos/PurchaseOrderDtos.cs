using ErpCrm.Domain.Enums;

namespace ErpCrm.Application.Purchase.Dtos;

public record PurchaseOrderLineDto(Guid? ProductId, string Description, decimal Quantity, decimal UnitCost, decimal TaxPercent = 0);

public record CreatePurchaseOrderDto(
    Guid VendorId,
    DateTime? OrderDate = null,
    DateTime? ExpectedDeliveryDate = null,
    string? CurrencyCode = null,
    string? Notes = null,
    List<PurchaseOrderLineDto>? Lines = null
);

public record UpdatePurchaseOrderStatusDto(PurchaseOrderStatus Status);

public class PurchaseOrderResponseDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public Guid VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public PurchaseOrderStatus Status { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? CurrencyCode { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<PurchaseOrderLineResponseDto> Lines { get; set; } = new();
}

public class PurchaseOrderLineResponseDto
{
    public Guid Id { get; set; }
    public Guid? ProductId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TaxPercent { get; set; }
    public decimal LineTotal { get; set; }
}
