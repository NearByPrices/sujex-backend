using ErpCrm.Domain.Common;
using ErpCrm.Domain.Enums;

namespace ErpCrm.Domain.Entities.Sales;

public class Payment : TenantEntity
{
    public string PaymentNumber { get; set; } = string.Empty;
    public Guid? SalesInvoiceId { get; set; }
    public Guid? ContactId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMode PaymentMode { get; set; } = PaymentMode.Cash;
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public string? Reference { get; set; }
    public string? Notes { get; set; }

    public SalesInvoice? SalesInvoice { get; set; }
}
