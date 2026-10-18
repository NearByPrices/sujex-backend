using Microsoft.AspNetCore.Identity;

namespace ErpCrm.Domain.Entities.Identity;

public class AppRole : IdentityRole<Guid>
{
    public Guid? TenantId { get; set; }
    public string? Description { get; set; }

    public ICollection<AppUserRole> UserRoles { get; set; } = new List<AppUserRole>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
