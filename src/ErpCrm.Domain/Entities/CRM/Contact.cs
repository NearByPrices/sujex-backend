using ErpCrm.Domain.Common;

namespace ErpCrm.Domain.Entities.CRM;

public class Contact : SoftDeleteEntity
{
    public Guid? CompanyId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public string? AccountName { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Notes { get; set; }
    public string? Tags { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Lead> Leads { get; set; } = new List<Lead>();
    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
}
