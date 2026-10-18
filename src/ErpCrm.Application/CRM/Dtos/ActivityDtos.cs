namespace ErpCrm.Application.CRM.Dtos;

public record CreateActivityDto(
    string Type,
    string Subject,
    string? Description,
    DateTime? DueDate,
    Guid? AssignedToUserId,
    Guid? ContactId,
    Guid? LeadId,
    Guid? OpportunityId
);

public record UpdateActivityDto(
    string Type,
    string Subject,
    string? Description,
    DateTime? DueDate,
    bool IsCompleted,
    DateTime? CompletedAt,
    Guid? AssignedToUserId
);

public record ActivityResponseDto(
    Guid Id,
    string Type,
    string Subject,
    string? Description,
    DateTime? DueDate,
    DateTime? CompletedAt,
    bool IsCompleted,
    Guid? AssignedToUserId,
    Guid? ContactId,
    Guid? LeadId,
    Guid? OpportunityId,
    DateTime CreatedAt
);
