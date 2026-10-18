using ErpCrm.Domain.Common;

namespace ErpCrm.Domain.Entities.Finance;

public class JournalLine : TenantEntity
{
    public Guid JournalEntryId { get; set; }
    public Guid AccountId { get; set; }
    public string? Description { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public int SortOrder { get; set; }

    public JournalEntry JournalEntry { get; set; } = null!;
    public Account Account { get; set; } = null!;
}
