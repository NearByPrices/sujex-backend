namespace ErpCrm.Domain.Entities.Foundation;

public class Branch : TenantEntity
{
    public Guid CompanyId { get; set; }
    public required string Name { get; set; }
    public string? Address { get; set; }

    public Company? Company { get; set; }
}
