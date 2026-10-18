using ErpCrm.Domain.Common;

namespace ErpCrm.Domain.Entities.HR;

public class LeaveType : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public int DaysAllowed { get; set; }
    public bool IsCarryForward { get; set; } = false;
    public int? MaxCarryForwardDays { get; set; }
    public bool IsPaid { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
}
