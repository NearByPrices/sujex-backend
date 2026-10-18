using ErpCrm.Application.Common.Interfaces;

namespace ErpCrm.Infrastructure.Persistence;

public class NullTenantContext : ITenantContext
{
    public Guid TenantId => Guid.Empty;
    public Guid? CompanyId => null;
    public Guid UserId => Guid.Empty;
    public bool IsPlatformAdmin => false;
}
