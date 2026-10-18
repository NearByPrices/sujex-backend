namespace ErpCrm.Application.Foundation.Dtos;

public record CreateCompanyDto(
    string Name,
    string BaseCurrency = "USD",
    string Timezone = "UTC",
    string? TaxId = null,
    string? Phone = null,
    string? Email = null,
    string? AddressLine1 = null,
    string? City = null,
    string? Country = null);

public record UpdateCompanyDto(
    string Name,
    string? TaxId,
    string? Website,
    string? Phone,
    string? Email,
    string? AddressLine1,
    string? AddressLine2,
    string? City,
    string? State,
    string? Country,
    string? PostalCode,
    string BaseCurrency,
    string Timezone,
    string DateFormat);

public class CompanyResponseDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? TaxId { get; set; }
    public string? Website { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? AddressLine1 { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string BaseCurrency { get; set; } = "USD";
    public string Timezone { get; set; } = "UTC";
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
