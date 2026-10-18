using ErpCrm.Domain.Enums;

namespace ErpCrm.Application.CRM.Dtos;

public record CreateOpportunityDto(
    string Title,
    Guid? ContactId,
    OpportunityStatus Status,
    decimal? ExpectedValue,
    int Probability,
    DateTime? ExpectedCloseDate,
    string? Stage,
    Guid? AssignedToUserId,
    string? Notes
);

public record UpdateOpportunityDto(
    string Title,
    Guid? ContactId,
    OpportunityStatus Status,
    decimal? ExpectedValue,
    decimal? ActualValue,
    int Probability,
    DateTime? ExpectedCloseDate,
    DateTime? ActualCloseDate,
    string? Stage,
    Guid? AssignedToUserId,
    string? Notes,
    string? LostReason
);

public record OpportunityResponseDto(
    Guid Id,
    string Title,
    Guid? ContactId,
    Guid? LeadId,
    OpportunityStatus Status,
    decimal? ExpectedValue,
    decimal? ActualValue,
    int Probability,
    DateTime? ExpectedCloseDate,
    DateTime? ActualCloseDate,
    string? Stage,
    Guid? AssignedToUserId,
    string? Notes,
    string? LostReason,
    DateTime CreatedAt
);
