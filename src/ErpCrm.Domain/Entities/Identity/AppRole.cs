using Microsoft.AspNetCore.Identity;

namespace ErpCrm.Domain.Entities.Identity;

public class AppRole : IdentityRole<Guid>
{
    public Guid? TenantId { get; set; }
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
