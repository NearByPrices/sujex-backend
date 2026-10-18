using ErpCrm.Domain.Common;

namespace ErpCrm.Domain.Entities.Foundation;

public class Company : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? TaxId { get; set; }
    public string? Website { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string BaseCurrency { get; set; } = "USD";
    public string Timezone { get; set; } = "UTC";
    public string DateFormat { get; set; } = "MM/dd/yyyy";
    public string FiscalYearStart { get; set; } = "01-01";
    public bool IsActive { get; set; } = true;
    public string? CustomFields { get; set; }

    public ICollection<Branch> Branches { get; set; } = new List<Branch>();
}
