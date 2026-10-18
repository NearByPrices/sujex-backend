namespace ErpCrm.Domain.Entities.Platform;

public class PlanModule : BaseEntity
{
    public Guid PlanId { get; set; }
    public required string ModuleCode { get; set; }

    public SubscriptionPlan? Plan { get; set; }
}
