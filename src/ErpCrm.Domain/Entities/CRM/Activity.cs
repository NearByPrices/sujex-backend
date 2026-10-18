using ErpCrm.Domain.Common;

namespace ErpCrm.Domain.Entities.CRM;

public class Activity : SoftDeleteEntity
{
    public string Type { get; set; } = string.Empty; // Call, Email, Meeting, Note, Task
    public string Subject { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsCompleted { get; set; } = false;
    public Guid? AssignedToUserId { get; set; }
    public Guid? ContactId { get; set; }
    public Guid? LeadId { get; set; }
    public Guid? OpportunityId { get; set; }

    public Contact? Contact { get; set; }
    public Lead? Lead { get; set; }
    public Opportunity? Opportunity { get; set; }
}
