namespace ErpCrm.Application.Projects.Dtos;

public record CreateProjectDto(
    string Name,
    string? Code = null,
    string? Description = null,
    Guid? CompanyId = null,
    Guid? ContactId = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    decimal? Budget = null,
    Guid? ManagerId = null,
    string? Tags = null
);

public record UpdateProjectDto(
    string Name,
    string? Code,
    string? Description,
    DateTime? StartDate,
    DateTime? EndDate,
    string Status,
    decimal? Budget,
    Guid? ManagerId,
    Guid? ContactId
);

public class ProjectResponseDto
{
    public Guid Id { get; set; }
    public Guid? CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = "Active";
    public decimal? Budget { get; set; }
    public Guid? ManagerId { get; set; }
    public Guid? ContactId { get; set; }
    public string? Tags { get; set; }
    public int TaskCount { get; set; }
    public int CompletedTaskCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
