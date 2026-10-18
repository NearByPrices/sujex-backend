namespace ErpCrm.Domain.Entities.Platform;

public class SubscriptionPlan : BaseEntity
{
    public required string Name { get; set; }
    public decimal Price { get; set; }
    public BillingCycle BillingCycle { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<PlanModule> PlanModules { get; set; } = new List<PlanModule>();
    public ICollection<TenantSubscription> TenantSubscriptions { get; set; } = new List<TenantSubscription>();
}

public enum BillingCycle
{
    Monthly,
    Quarterly,
    Annually
}
