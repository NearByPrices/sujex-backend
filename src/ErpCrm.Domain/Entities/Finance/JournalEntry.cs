using ErpCrm.Domain.Common;

namespace ErpCrm.Domain.Entities.Finance;

public class JournalEntry : TenantEntity
{
    public string EntryNumber { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; } = DateTime.UtcNow;
    public string? Description { get; set; }
    public string? Reference { get; set; }
    public string? ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }
    public bool IsPosted { get; set; } = false;
    public DateTime? PostedAt { get; set; }
    public Guid? PostedByUserId { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }

    public ICollection<JournalLine> Lines { get; set; } = new List<JournalLine>();
}
