using ErpCrm.Domain.Common;

namespace ErpCrm.Domain.Entities.Identity;

public class UserCompanyRole : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid CompanyId { get; set; }
    public Guid RoleId { get; set; }
    public bool IsActive { get; set; } = true;

    public AppUser User { get; set; } = null!;
    public AppRole Role { get; set; } = null!;
}
