using ErpCrm.Domain.Enums;

namespace ErpCrm.Application.Sales.Dtos;

public record CreateSalesInvoiceDto(
    Guid CompanyId,
    Guid? SalesOrderId = null,
    Guid? ContactId = null,
    DateTime? InvoiceDate = null,
    DateTime? DueDate = null,
    string CurrencyCode = "USD",
    string? Notes = null
);

public record UpdateSalesInvoiceStatusDto(InvoiceStatus Status, string? PaymentReference = null);

public class SalesInvoiceResponseDto
{
    public Guid Id { get; set; }
    public Guid? CompanyId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid? SalesOrderId { get; set; }
    public string? SalesOrderNumber { get; set; }
    public Guid? ContactId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public InvoiceStatus Status { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceDue { get; set; }
    public string? CurrencyCode { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
