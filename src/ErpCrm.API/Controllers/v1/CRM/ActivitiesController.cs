using ErpCrm.Application.Common.Interfaces;
using ErpCrm.Application.Common.Models;
using ErpCrm.Application.CRM.Dtos;
using ErpCrm.Domain.Entities.CRM;
using ErpCrm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpCrm.API.Controllers.v1.CRM;

[Authorize]
[ApiController]
[Route("api/v1/activities")]
public class ActivitiesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenant;

    public ActivitiesController(AppDbContext db, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] Guid? contactId = null, [FromQuery] Guid? leadId = null, [FromQuery] Guid? opportunityId = null)
    {
        var query = _db.Activities.Where(a => !a.IsDeleted);
        if (contactId.HasValue) query = query.Where(a => a.ContactId == contactId);
        if (leadId.HasValue) query = query.Where(a => a.LeadId == leadId);
        if (opportunityId.HasValue) query = query.Where(a => a.OpportunityId == opportunityId);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(a => new ActivityResponseDto(a.Id, a.Type, a.Subject, a.Description, a.DueDate, a.CompletedAt, a.IsCompleted, a.AssignedToUserId, a.ContactId, a.LeadId, a.OpportunityId, a.CreatedAt))
            .ToListAsync();

        return Ok(ApiResponse<List<ActivityResponseDto>>.OkPaged(items, new PaginationMeta { Page = page, PageSize = pageSize, Total = total }));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateActivityDto dto)
    {
        var activity = new Activity
        {
            TenantId = _tenant.TenantId,
            Type = dto.Type,
            Subject = dto.Subject,
            Description = dto.Description,
            DueDate = dto.DueDate,
            AssignedToUserId = dto.AssignedToUserId,
            ContactId = dto.ContactId,
            LeadId = dto.LeadId,
            OpportunityId = dto.OpportunityId
        };
        _db.Activities.Add(activity);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<ActivityResponseDto>.Ok(new ActivityResponseDto(activity.Id, activity.Type, activity.Subject, activity.Description, activity.DueDate, activity.CompletedAt, activity.IsCompleted, activity.AssignedToUserId, activity.ContactId, activity.LeadId, activity.OpportunityId, activity.CreatedAt)));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateActivityDto dto)
    {
        var activity = await _db.Activities.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (activity == null) return NotFound(ApiResponse<object>.Fail("Activity not found"));

        activity.Type = dto.Type;
        activity.Subject = dto.Subject;
        activity.Description = dto.Description;
        activity.DueDate = dto.DueDate;
        activity.IsCompleted = dto.IsCompleted;
        activity.CompletedAt = dto.CompletedAt;
        activity.AssignedToUserId = dto.AssignedToUserId;

        await _db.SaveChangesAsync();
        return Ok(ApiResponse<ActivityResponseDto>.Ok(new ActivityResponseDto(activity.Id, activity.Type, activity.Subject, activity.Description, activity.DueDate, activity.CompletedAt, activity.IsCompleted, activity.AssignedToUserId, activity.ContactId, activity.LeadId, activity.OpportunityId, activity.CreatedAt)));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var activity = await _db.Activities.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (activity == null) return NotFound(ApiResponse<object>.Fail("Activity not found"));
        activity.IsDeleted = true;
        activity.DeletedAt = DateTime.UtcNow;
        activity.DeletedBy = _tenant.UserId;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(null!, "Activity deleted"));
    }
}
