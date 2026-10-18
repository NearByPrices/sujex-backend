using ErpCrm.Domain.Common;

namespace ErpCrm.Domain.Entities.Foundation;

public class NumberSequence : TenantEntity
{
    public Guid CompanyId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string Prefix { get; set; } = string.Empty;
    public long LastNumber { get; set; } = 0;
    public int PaddingLength { get; set; } = 5;
    public string? Suffix { get; set; }

    public string GetNext()
    {
        LastNumber++;
        var padded = LastNumber.ToString().PadLeft(PaddingLength, '0');
        return $"{Prefix}{padded}{Suffix}";
    }
}
