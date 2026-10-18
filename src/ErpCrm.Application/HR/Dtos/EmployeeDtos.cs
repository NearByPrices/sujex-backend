using ErpCrm.Domain.Enums;

namespace ErpCrm.Application.HR.Dtos;

public record CreateEmployeeDto(
    Guid? CompanyId,
    string FirstName,
    string LastName,
    string? Email = null,
    string? Phone = null,
    DateTime? DateOfBirth = null,
    DateTime? JoinDate = null,
    Guid? DepartmentId = null,
    Guid? DesignationId = null,
    Guid? ManagerId = null,
    decimal? BasicSalary = null,
    string? NationalId = null,
    string? BankAccount = null,
    Guid? UserId = null
);

public record UpdateEmployeeDto(
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    DateTime? DateOfBirth,
    DateTime JoinDate,
    Guid? DepartmentId,
    Guid? DesignationId,
    Guid? ManagerId,
    EmployeeStatus Status,
    decimal? BasicSalary,
    string? NationalId,
    string? BankAccount
);

public class EmployeeResponseDto
{
    public Guid Id { get; set; }
    public Guid? CompanyId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public DateTime JoinDate { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public Guid? DesignationId { get; set; }
    public string? DesignationName { get; set; }
    public Guid? ManagerId { get; set; }
    public EmployeeStatus Status { get; set; }
    public decimal? BasicSalary { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}
