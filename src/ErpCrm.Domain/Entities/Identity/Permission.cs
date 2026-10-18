namespace ErpCrm.Domain.Entities.Identity;

public class Permission : BaseEntity
{
    public required string ModuleCode { get; set; }
    public required string PermissionCode { get; set; }
    public string? Description { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
