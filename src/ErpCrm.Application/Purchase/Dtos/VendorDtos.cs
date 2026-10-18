namespace ErpCrm.Application.Purchase.Dtos;

public record CreateVendorDto(string Name, string? Code = null, string? Email = null, string? Phone = null, string? Address = null, string? TaxNumber = null, string? CurrencyCode = null, int? PaymentTermsDays = null, string? Notes = null);
public record UpdateVendorDto(string Name, string? Code, string? Email, string? Phone, string? Address, string? TaxNumber, string? CurrencyCode, int? PaymentTermsDays, bool IsActive, string? Notes);

public class VendorResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? TaxNumber { get; set; }
    public string? CurrencyCode { get; set; }
    public int? PaymentTermsDays { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
