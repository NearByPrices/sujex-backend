using Microsoft.AspNetCore.Identity;

namespace ErpCrm.Domain.Entities.Identity;

public class AppUserRole : IdentityUserRole<Guid>
{
    public AppUser? User { get; set; }
    public AppRole? Role { get; set; }
}
