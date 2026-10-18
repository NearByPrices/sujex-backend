using Microsoft.AspNetCore.Identity;

namespace ErpCrm.Domain.Entities.Identity;

public class AppUser : IdentityUser<Guid>, IHasAudit
{
    public Guid? TenantId { get; set; }
    public bool IsActive { get; set; } = true;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }

    public ICollection<AppUserRole> UserRoles { get; set; } = new List<AppUserRole>();
    public ICollection<UserCompanyRole> UserCompanyRoles { get; set; } = new List<UserCompanyRole>();
}

public interface IHasAudit
{
    DateTime CreatedAt { get; set; }
    Guid CreatedBy { get; set; }
    DateTime? UpdatedAt { get; set; }
    Guid? UpdatedBy { get; set; }
}
