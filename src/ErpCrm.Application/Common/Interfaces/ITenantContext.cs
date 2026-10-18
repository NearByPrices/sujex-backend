namespace ErpCrm.Application.Common.Interfaces;

public interface ITenantContext
{
    Guid TenantId { get; }
    Guid? CompanyId { get; }
    Guid UserId { get; }
    bool IsPlatformAdmin { get; }
}
