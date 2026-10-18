namespace ErpCrm.Application.Projects.Dtos;

public record CreateTaskDto(
    string Title,
    string? Description = null,
    string Priority = "Medium",
    DateTime? DueDate = null,
    Guid? AssignedToUserId = null,
    Guid? ParentTaskId = null,
    int? EstimatedHours = null
);

public record UpdateTaskDto(
    string Title,
    string? Description,
    string Status,
    string Priority,
    DateTime? DueDate,
    Guid? AssignedToUserId,
    int? EstimatedHours,
    int? ActualHours
);

public class TaskResponseDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "Todo";
    public string Priority { get; set; } = "Medium";
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public Guid? ParentTaskId { get; set; }
    public int? EstimatedHours { get; set; }
    public int? ActualHours { get; set; }
    public int SubTaskCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
