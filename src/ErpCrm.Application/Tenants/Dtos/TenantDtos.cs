using ErpCrm.Domain.Enums;

namespace ErpCrm.Application.Tenants.Dtos;

public record CreateTenantDto(
    string Name,
    string Slug,
    string? ContactEmail,
    string? ContactPhone,
    int MaxUsers = 10,
    int MaxCompanies = 1);

public record UpdateTenantDto(
    string Name,
    string? ContactEmail,
    string? ContactPhone,
    int MaxUsers,
    int MaxCompanies);

public class TenantResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public TenantStatus Status { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? LogoUrl { get; set; }
    public int MaxUsers { get; set; }
    public int MaxCompanies { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TenantListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public TenantStatus Status { get; set; }
    public string? ContactEmail { get; set; }
    public DateTime CreatedAt { get; set; }
}
