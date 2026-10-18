namespace ErpCrm.Domain.Entities.Platform;

public class Tenant : BaseEntity
{
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public TenantStatus Status { get; set; } = TenantStatus.Active;

    public ICollection<SubscriptionPlan> SubscriptionPlans { get; set; } = new List<SubscriptionPlan>();
}

public enum TenantStatus
{
    Active,
    Suspended,
    Cancelled
}
