using ErpCrm.Domain.Common;
using ErpCrm.Domain.Enums;

namespace ErpCrm.Domain.Entities.CRM;

public class Lead : SoftDeleteEntity
{
    public string Title { get; set; } = string.Empty;
    public Guid? ContactId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Company { get; set; }
    public string? JobTitle { get; set; }
    public LeadStatusEnum Status { get; set; } = LeadStatusEnum.New;
    public string? Source { get; set; }
    public decimal? EstimatedValue { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string? Notes { get; set; }
    public string? Tags { get; set; }
    public DateTime? FollowUpDate { get; set; }
    public Guid? ConvertedOpportunityId { get; set; }

    public Contact? Contact { get; set; }
    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
}
