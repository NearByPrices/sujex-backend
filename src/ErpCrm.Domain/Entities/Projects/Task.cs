using ErpCrm.Domain.Common;

namespace ErpCrm.Domain.Entities.Projects;

public class Task : SoftDeleteEntity
{
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "Todo"; // Todo, InProgress, Done, Cancelled
    public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical
    public Guid? AssignedToUserId { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? EstimatedHours { get; set; }
    public int? ActualHours { get; set; }
    public Guid? ParentTaskId { get; set; }
    public Task? ParentTask { get; set; }
    public ICollection<Task> SubTasks { get; set; } = new List<Task>();

    public Project Project { get; set; } = null!;
}
