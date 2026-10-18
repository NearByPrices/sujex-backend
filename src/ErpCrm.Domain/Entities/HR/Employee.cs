using ErpCrm.Domain.Common;
using ErpCrm.Domain.Enums;

namespace ErpCrm.Domain.Entities.HR;

public class Employee : SoftDeleteEntity
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? NationalId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? DesignationId { get; set; }
    public Guid? ManagerId { get; set; }
    public Guid? UserId { get; set; }
    public Guid? CompanyId { get; set; }
    public DateTime JoinDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;
    public string? Address { get; set; }
    public decimal? BasicSalary { get; set; }
    public string? BankAccount { get; set; }
    public string? AvatarUrl { get; set; }

    public Department? Department { get; set; }
    public Designation? Designation { get; set; }
    public Employee? Manager { get; set; }
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
}
