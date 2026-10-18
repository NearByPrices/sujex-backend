using ErpCrm.Domain.Common;

namespace ErpCrm.Domain.Entities.Platform;

public class BillingInvoice : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid SubscriptionId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Pending"; // Pending/Paid/Overdue
    public string? PaymentReference { get; set; }
    public DateTime? PaidAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public TenantSubscription Subscription { get; set; } = null!;
}
