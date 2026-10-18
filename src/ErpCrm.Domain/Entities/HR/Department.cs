using ErpCrm.Domain.Common;

namespace ErpCrm.Domain.Entities.HR;

public class Department : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid? ParentId { get; set; }
    public Department? Parent { get; set; }
    public ICollection<Department> Children { get; set; } = new List<Department>();
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
