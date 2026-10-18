using ErpCrm.Domain.Common;

namespace ErpCrm.Domain.Entities.Finance;

public class Account : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty; // Asset, Liability, Equity, Income, Expense
    public Guid? ParentId { get; set; }
    public Account? Parent { get; set; }
    public ICollection<Account> Children { get; set; } = new List<Account>();
    public decimal Balance { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsSystemAccount { get; set; } = false;
    public ICollection<JournalLine> JournalLines { get; set; } = new List<JournalLine>();
}
