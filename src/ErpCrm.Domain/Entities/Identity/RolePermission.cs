namespace ErpCrm.Domain.Entities.Identity;

public class RolePermission : BaseEntity
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }

    public AppRole? Role { get; set; }
    public Permission? Permission { get; set; }
}
