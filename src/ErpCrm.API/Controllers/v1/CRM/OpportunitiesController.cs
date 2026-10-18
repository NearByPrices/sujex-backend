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
[Route("api/v1/opportunities")]
public class OpportunitiesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenant;

    public OpportunitiesController(AppDbContext db, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
    {
        var query = _db.Opportunities.Where(o => !o.IsDeleted);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(o => o.Title.Contains(search));

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(o => new OpportunityResponseDto(o.Id, o.Title, o.ContactId, o.LeadId, o.Status, o.ExpectedValue, o.ActualValue, o.Probability, o.ExpectedCloseDate, o.ActualCloseDate, o.Stage, o.AssignedToUserId, o.Notes, o.LostReason, o.CreatedAt))
            .ToListAsync();

        return Ok(ApiResponse<List<OpportunityResponseDto>>.OkPaged(items, new PaginationMeta { Page = page, PageSize = pageSize, TotalCount = total }));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var o = await _db.Opportunities.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (o == null) return NotFound(ApiResponse<object>.Fail("Opportunity not found"));
        return Ok(ApiResponse<OpportunityResponseDto>.Ok(new OpportunityResponseDto(o.Id, o.Title, o.ContactId, o.LeadId, o.Status, o.ExpectedValue, o.ActualValue, o.Probability, o.ExpectedCloseDate, o.ActualCloseDate, o.Stage, o.AssignedToUserId, o.Notes, o.LostReason, o.CreatedAt)));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOpportunityDto dto)
    {
        var opp = new Opportunity
        {
            TenantId = _tenant.TenantId,
            Title = dto.Title,
            ContactId = dto.ContactId,
            Status = dto.Status,
            ExpectedValue = dto.ExpectedValue,
            Probability = dto.Probability,
            ExpectedCloseDate = dto.ExpectedCloseDate,
            Stage = dto.Stage,
            AssignedToUserId = dto.AssignedToUserId,
            Notes = dto.Notes
        };
        _db.Opportunities.Add(opp);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = opp.Id },
            ApiResponse<OpportunityResponseDto>.Ok(new OpportunityResponseDto(opp.Id, opp.Title, opp.ContactId, opp.LeadId, opp.Status, opp.ExpectedValue, opp.ActualValue, opp.Probability, opp.ExpectedCloseDate, opp.ActualCloseDate, opp.Stage, opp.AssignedToUserId, opp.Notes, opp.LostReason, opp.CreatedAt)));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOpportunityDto dto)
    {
        var opp = await _db.Opportunities.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (opp == null) return NotFound(ApiResponse<object>.Fail("Opportunity not found"));

        opp.Title = dto.Title;
        opp.ContactId = dto.ContactId;
        opp.Status = dto.Status;
        opp.ExpectedValue = dto.ExpectedValue;
        opp.ActualValue = dto.ActualValue;
        opp.Probability = dto.Probability;
        opp.ExpectedCloseDate = dto.ExpectedCloseDate;
        opp.ActualCloseDate = dto.ActualCloseDate;
        opp.Stage = dto.Stage;
        opp.AssignedToUserId = dto.AssignedToUserId;
        opp.Notes = dto.Notes;
        opp.LostReason = dto.LostReason;

        await _db.SaveChangesAsync();
        return Ok(ApiResponse<OpportunityResponseDto>.Ok(new OpportunityResponseDto(opp.Id, opp.Title, opp.ContactId, opp.LeadId, opp.Status, opp.ExpectedValue, opp.ActualValue, opp.Probability, opp.ExpectedCloseDate, opp.ActualCloseDate, opp.Stage, opp.AssignedToUserId, opp.Notes, opp.LostReason, opp.CreatedAt)));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var opp = await _db.Opportunities.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (opp == null) return NotFound(ApiResponse<object>.Fail("Opportunity not found"));
        opp.IsDeleted = true;
        opp.DeletedAt = DateTime.UtcNow;
        opp.DeletedBy = _tenant.UserId;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(null!, "Opportunity deleted"));
    }
}
