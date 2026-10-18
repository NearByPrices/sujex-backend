namespace ErpCrm.Domain.Entities.Platform;

public class Tenant : BaseEntity
{
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public TenantStatus Status { get; set; } = TenantStatus.Active;

    public ICollection<TenantSubscription> TenantSubscriptions { get; set; } = new List<TenantSubscription>();
}

public enum TenantStatus
{
    Active,
    Suspended,
    Cancelled
}
