using ErpCrm.Domain.Enums;

namespace ErpCrm.Application.CRM.Dtos;

public record CreateLeadDto(
    string Title,
    string? FirstName,
    string? LastName,
    string? Email,
    string? Phone,
    string? Company,
    string? JobTitle,
    LeadStatusEnum Status,
    string? Source,
    decimal? EstimatedValue,
    Guid? AssignedToUserId,
    string? Notes,
    string? Tags,
    DateTime? FollowUpDate,
    Guid? ContactId
);

public record UpdateLeadDto(
    string Title,
    string? FirstName,
    string? LastName,
    string? Email,
    string? Phone,
    string? Company,
    string? JobTitle,
    LeadStatusEnum Status,
    string? Source,
    decimal? EstimatedValue,
    Guid? AssignedToUserId,
    string? Notes,
    string? Tags,
    DateTime? FollowUpDate
);

public record LeadResponseDto(
    Guid Id,
    string Title,
    string? FirstName,
    string? LastName,
    string? Email,
    string? Phone,
    string? Company,
    LeadStatusEnum Status,
    string? Source,
    decimal? EstimatedValue,
    Guid? AssignedToUserId,
    string? Tags,
    DateTime? FollowUpDate,
    Guid? ConvertedOpportunityId,
    DateTime CreatedAt
);

public record ConvertLeadDto(
    string OpportunityTitle,
    decimal? ExpectedValue,
    DateTime? ExpectedCloseDate
);
