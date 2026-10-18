using ErpCrm.Domain.Common;
using ErpCrm.Domain.Enums;

namespace ErpCrm.Domain.Entities.Platform;

public class Tenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public TenantStatus Status { get; set; } = TenantStatus.Active;
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? LogoUrl { get; set; }
    public int MaxUsers { get; set; } = 10;
    public int MaxCompanies { get; set; } = 1;

    public ICollection<TenantSubscription> TenantSubscriptions { get; set; } = new List<TenantSubscription>();
    public ICollection<TenantModule> TenantModules { get; set; } = new List<TenantModule>();
}
