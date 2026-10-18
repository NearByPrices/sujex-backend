namespace ErpCrm.Application.Foundation.Dtos;

public record CreateBranchDto(
    Guid CompanyId,
    string Name,
    string Code,
    string? Phone = null,
    string? Email = null,
    string? AddressLine1 = null,
    string? City = null,
    string? Country = null,
    bool IsHeadOffice = false);

public record UpdateBranchDto(
    string Name,
    string Code,
    string? Phone,
    string? Email,
    string? AddressLine1,
    string? City,
    string? Country,
    bool IsHeadOffice);

public class BranchResponseDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? AddressLine1 { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public bool IsActive { get; set; }
    public bool IsHeadOffice { get; set; }
}
