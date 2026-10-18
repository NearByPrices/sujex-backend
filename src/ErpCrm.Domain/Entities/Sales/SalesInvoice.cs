using ErpCrm.Domain.Common;
using ErpCrm.Domain.Enums;

namespace ErpCrm.Domain.Entities.Sales;

public class SalesInvoice : SoftDeleteEntity
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid? SalesOrderId { get; set; }
    public Guid? ContactId { get; set; }
    public Guid? CompanyId { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
    public DateTime DueDate { get; set; }
    public string? CurrencyCode { get; set; }
    public decimal ExchangeRate { get; set; } = 1;
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceDue => TotalAmount - PaidAmount;
    public string? Notes { get; set; }
    public string? BillingAddress { get; set; }

    public SalesOrder? SalesOrder { get; set; }
    public ICollection<SalesInvoiceLine> Lines { get; set; } = new List<SalesInvoiceLine>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
