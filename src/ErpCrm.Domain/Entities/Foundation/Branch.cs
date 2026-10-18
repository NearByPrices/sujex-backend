using ErpCrm.Domain.Common;

namespace ErpCrm.Domain.Entities.Foundation;

public class Branch : TenantEntity
{
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? AddressLine1 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsHeadOffice { get; set; } = false;

    public Company Company { get; set; } = null!;
}
