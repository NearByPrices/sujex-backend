namespace ErpCrm.Domain.Common;

public abstract class SoftDeleteEntity : TenantEntity
{
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}
