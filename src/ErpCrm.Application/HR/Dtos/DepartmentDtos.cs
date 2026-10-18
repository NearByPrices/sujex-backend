namespace ErpCrm.Application.HR.Dtos;

public record CreateDepartmentDto(Guid CompanyId, string Name, string? Code = null, Guid? ManagerId = null);
public record UpdateDepartmentDto(string Name, string? Code, Guid? ManagerId, bool IsActive);

public class DepartmentResponseDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public Guid? ManagerId { get; set; }
    public bool IsActive { get; set; }
    public int EmployeeCount { get; set; }
}
