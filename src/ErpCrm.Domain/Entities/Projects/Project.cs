using ErpCrm.Domain.Common;

namespace ErpCrm.Domain.Entities.Projects;

public class Project : SoftDeleteEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public Guid? ContactId { get; set; }
    public Guid? CompanyId { get; set; }
    public string Status { get; set; } = "Active"; // Active, OnHold, Completed, Cancelled
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? Budget { get; set; }
    public Guid? ManagerId { get; set; }
    public string? Tags { get; set; }

    public ICollection<Task> Tasks { get; set; } = new List<Task>();
}
