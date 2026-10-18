namespace ErpCrm.Application.Finance.Dtos;

public record CreateAccountDto(string Code, string Name, string AccountType, Guid? ParentId = null, string? Description = null);
public record UpdateAccountDto(string Code, string Name, string AccountType, Guid? ParentId, bool IsActive, string? Description);

public class AccountResponseDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
    public string? ParentName { get; set; }
    public decimal Balance { get; set; }
    public bool IsActive { get; set; }
    public bool IsSystemAccount { get; set; }
    public string? Description { get; set; }
}
