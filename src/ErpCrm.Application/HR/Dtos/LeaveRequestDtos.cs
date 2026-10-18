using ErpCrm.Domain.Enums;

namespace ErpCrm.Application.HR.Dtos;

public record CreateLeaveRequestDto(Guid EmployeeId, Guid LeaveTypeId, DateTime StartDate, DateTime EndDate, string? Reason = null);
public record ApproveLeaveRequestDto(bool IsApproved, string? ApprovalNotes = null);

public class LeaveRequestResponseDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public Guid LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalDays { get; set; }
    public string? Reason { get; set; }
    public ApprovalStatus Status { get; set; }
    public string? ApprovalNotes { get; set; }
    public DateTime CreatedAt { get; set; }
}
