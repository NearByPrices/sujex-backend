namespace ErpCrm.Application.Common.Interfaces;

public interface ICurrentUser
{
    Guid UserId { get; }
    string Email { get; }
    string FullName { get; }
    Guid TenantId { get; }
    Guid? CompanyId { get; }
    bool IsPlatformAdmin { get; }
    IEnumerable<string> Permissions { get; }
}
