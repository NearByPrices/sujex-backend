using ErpCrm.Domain.Common;
using ErpCrm.Domain.Enums;

namespace ErpCrm.Domain.Entities.Platform;

public class PlanModule : BaseEntity
{
    public Guid PlanId { get; set; }
    public ModuleCode ModuleCode { get; set; }
    public int? MaxUsers { get; set; }
    public bool IsIncluded { get; set; } = true;

    public SubscriptionPlan Plan { get; set; } = null!;
}
