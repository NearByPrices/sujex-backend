namespace ErpCrm.Domain.Entities.Identity;

public class UserCompanyRole : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid CompanyId { get; set; }
    public Guid RoleId { get; set; }

    public AppUser? User { get; set; }
    public AppRole? Role { get; set; }
}
