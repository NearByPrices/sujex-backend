using ErpCrm.Domain.Common;
using ErpCrm.Domain.Enums;

namespace ErpCrm.Domain.Entities.Platform;

public class SubscriptionPlan : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public BillingCycle BillingCycle { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsCustom { get; set; } = false;

    public ICollection<PlanModule> PlanModules { get; set; } = new List<PlanModule>();
    public ICollection<TenantSubscription> TenantSubscriptions { get; set; } = new List<TenantSubscription>();
}
