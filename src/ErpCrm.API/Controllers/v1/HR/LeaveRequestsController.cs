using ErpCrm.Application.Common.Models;
using ErpCrm.Application.Common.Interfaces;
using ErpCrm.Application.HR.Dtos;
using ErpCrm.Domain.Entities.HR;
using ErpCrm.Domain.Enums;
using ErpCrm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpCrm.API.Controllers.v1.HR;

[ApiController]
[Route("api/v1/hr/leave-requests")]
[Authorize]
public class LeaveRequestsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;

    public LeaveRequestsController(AppDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<LeaveRequestResponseDto>>>> GetAll([FromQuery] QueryParams q, [FromQuery] ApprovalStatus? status = null, [FromQuery] Guid? employeeId = null)
    {
        var query = _db.LeaveRequests.AsNoTracking();
        if (status.HasValue) query = query.Where(r => r.Status == status);
        if (employeeId.HasValue) query = query.Where(r => r.EmployeeId == employeeId);

        var total = await query.CountAsync();
        var items = await query.Include(r => r.Employee).Include(r => r.LeaveType)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((q.Page - 1) * q.PageSize).Take(q.PageSize)
            .Select(r => new LeaveRequestResponseDto
            {
                Id = r.Id, EmployeeId = r.EmployeeId,
                EmployeeName = r.Employee.FirstName + " " + r.Employee.LastName,
                LeaveTypeId = r.LeaveTypeId, LeaveTypeName = r.LeaveType.Name,
                StartDate = r.StartDate, EndDate = r.EndDate, TotalDays = r.TotalDays,
                Reason = r.Reason, Status = r.Status, ApprovalNotes = r.ApprovalNotes,
                CreatedAt = r.CreatedAt
            }).ToListAsync();

        var pagination = new PaginationMeta { Page = q.Page, PageSize = q.PageSize, TotalCount = total };
        return Ok(ApiResponse<PagedResult<LeaveRequestResponseDto>>.OkPaged(
            new PagedResult<LeaveRequestResponseDto> { Items = items, Pagination = pagination }, pagination));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<LeaveRequestResponseDto>>> Create([FromBody] CreateLeaveRequestDto dto)
    {
        var totalDays = (int)(dto.EndDate - dto.StartDate).TotalDays + 1;
        var request = new LeaveRequest
        {
            TenantId = _tenantContext.TenantId, EmployeeId = dto.EmployeeId,
            LeaveTypeId = dto.LeaveTypeId, StartDate = dto.StartDate, EndDate = dto.EndDate,
            TotalDays = totalDays, Reason = dto.Reason
        };
        _db.LeaveRequests.Add(request);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<LeaveRequestResponseDto>.Ok(new LeaveRequestResponseDto
        {
            Id = request.Id, EmployeeId = request.EmployeeId, LeaveTypeId = request.LeaveTypeId,
            StartDate = request.StartDate, EndDate = request.EndDate, TotalDays = request.TotalDays,
            Reason = request.Reason, Status = request.Status, CreatedAt = request.CreatedAt
        }));
    }

    [HttpPatch("{id:guid}/approve")]
    public async Task<ActionResult<ApiResponse<string>>> Approve(Guid id, [FromBody] ApproveLeaveRequestDto dto)
    {
        var request = await _db.LeaveRequests.FindAsync(id) ?? throw new KeyNotFoundException("Leave request not found");
        request.Status = dto.IsApproved ? ApprovalStatus.Approved : ApprovalStatus.Rejected;
        request.ApprovedByUserId = _tenantContext.UserId;
        request.ApprovedAt = DateTime.UtcNow;
        request.ApprovalNotes = dto.ApprovalNotes;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<string>.Ok(dto.IsApproved ? "Leave approved" : "Leave rejected"));
    }
}
