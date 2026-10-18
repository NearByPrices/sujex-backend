using ErpCrm.Domain.Common;

namespace ErpCrm.Domain.Entities.Sales;

public class SalesInvoiceLine : TenantEntity
{
    public Guid SalesInvoiceId { get; set; }
    public Guid? ProductId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal TaxPercent { get; set; }
    public decimal LineTotal => Quantity * UnitPrice * (1 - DiscountPercent / 100) * (1 + TaxPercent / 100);
    public int SortOrder { get; set; }

    public SalesInvoice SalesInvoice { get; set; } = null!;
}
