using ErpCrm.Application.Common.Interfaces;

namespace ErpCrm.Infrastructure.Services;

public class TenantContext : ITenantContext
{
    public Guid TenantId { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid UserId { get; set; }
    public bool IsPlatformAdmin { get; set; }
}
