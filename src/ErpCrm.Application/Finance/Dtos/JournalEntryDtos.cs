namespace ErpCrm.Application.Finance.Dtos;

public record JournalLineDto(Guid AccountId, string? Description, decimal Debit, decimal Credit);

public record CreateJournalEntryDto(
    DateTime EntryDate,
    string? Description = null,
    string? Reference = null,
    string? ReferenceType = null,
    List<JournalLineDto>? Lines = null
);

public class JournalEntryResponseDto
{
    public Guid Id { get; set; }
    public string EntryNumber { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; }
    public string? Description { get; set; }
    public string? Reference { get; set; }
    public string? ReferenceType { get; set; }
    public bool IsPosted { get; set; }
    public DateTime? PostedAt { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<JournalLineResponseDto> Lines { get; set; } = new();
}

public class JournalLineResponseDto
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
}
