namespace ErpCrm.Application.CRM.Dtos;

public record CreateContactDto(
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    string? Mobile,
    string? JobTitle,
    string? Department,
    string? AccountName,
    string? Notes,
    string? Tags,
    Guid? CompanyId
);

public record UpdateContactDto(
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    string? Mobile,
    string? JobTitle,
    string? Department,
    string? AccountName,
    string? Notes,
    string? Tags,
    bool IsActive
);

public record ContactResponseDto(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    string? Email,
    string? Phone,
    string? Mobile,
    string? JobTitle,
    string? Department,
    string? AccountName,
    string? AvatarUrl,
    string? Notes,
    string? Tags,
    bool IsActive,
    Guid? CompanyId,
    DateTime CreatedAt
);
