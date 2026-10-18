using ErpCrm.Domain.Common;

namespace ErpCrm.Domain.Entities.Foundation;

public class Notification : TenantEntity
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = "Info"; // Info/Warning/Error/Success
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
    public string? ActionUrl { get; set; }
    public string? ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }
}
