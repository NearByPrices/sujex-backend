using ErpCrm.Domain.Common;
using ErpCrm.Domain.Enums;

namespace ErpCrm.Domain.Entities.CRM;

public class Opportunity : SoftDeleteEntity
{
    public string Title { get; set; } = string.Empty;
    public Guid? ContactId { get; set; }
    public Guid? LeadId { get; set; }
    public OpportunityStatus Status { get; set; } = OpportunityStatus.Open;
    public decimal? ExpectedValue { get; set; }
    public decimal? ActualValue { get; set; }
    public int Probability { get; set; } = 50;
    public DateTime? ExpectedCloseDate { get; set; }
    public DateTime? ActualCloseDate { get; set; }
    public string? Stage { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string? Notes { get; set; }
    public string? LostReason { get; set; }

    public Contact? Contact { get; set; }
    public Lead? Lead { get; set; }
    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
}
