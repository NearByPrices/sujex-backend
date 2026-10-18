namespace ErpCrm.Domain.Entities.Platform;

public class TenantSubscription : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid PlanId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;

    public SubscriptionPlan? Plan { get; set; }
}

public enum SubscriptionStatus
{
    Active,
    Expired,
    Suspended,
    Cancelled
}
