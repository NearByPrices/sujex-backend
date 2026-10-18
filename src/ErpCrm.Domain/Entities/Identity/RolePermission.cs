using ErpCrm.Domain.Common;

namespace ErpCrm.Domain.Entities.Identity;

public class RolePermission : BaseEntity
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }

    public AppRole Role { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}
