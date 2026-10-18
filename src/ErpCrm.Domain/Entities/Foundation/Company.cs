namespace ErpCrm.Domain.Entities.Foundation;

public class Company : TenantEntity
{
    public required string Name { get; set; }
    public string? LogoUrl { get; set; }
    public string? Address { get; set; }
    public required string BaseCurrency { get; set; } = "USD";
    public string? Timezone { get; set; } = "UTC";
    public string? TaxId { get; set; }

    public ICollection<Branch> Branches { get; set; } = new List<Branch>();
}
