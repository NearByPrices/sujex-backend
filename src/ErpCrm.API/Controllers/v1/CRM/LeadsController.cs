using ErpCrm.Application.Common.Interfaces;
using ErpCrm.Application.Common.Models;
using ErpCrm.Application.CRM.Dtos;
using ErpCrm.Domain.Entities.CRM;
using ErpCrm.Domain.Enums;
using ErpCrm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpCrm.API.Controllers.v1.CRM;

[Authorize]
[ApiController]
[Route("api/v1/leads")]
public class LeadsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenant;

    public LeadsController(AppDbContext db, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null, [FromQuery] LeadStatusEnum? status = null)
    {
        var query = _db.Leads.Where(l => !l.IsDeleted);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(l => l.Title.Contains(search) || l.Email!.Contains(search));
        if (status.HasValue)
            query = query.Where(l => l.Status == status.Value);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(l => new LeadResponseDto(l.Id, l.Title, l.FirstName, l.LastName, l.Email, l.Phone, l.Company, l.Status, l.Source, l.EstimatedValue, l.AssignedToUserId, l.Tags, l.FollowUpDate, l.ConvertedOpportunityId, l.CreatedAt))
            .ToListAsync();

        return Ok(ApiResponse<List<LeadResponseDto>>.OkPaged(items, new PaginationMeta { Page = page, PageSize = pageSize, TotalCount = total }));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var l = await _db.Leads.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (l == null) return NotFound(ApiResponse<object>.Fail("Lead not found"));
        return Ok(ApiResponse<LeadResponseDto>.Ok(new LeadResponseDto(l.Id, l.Title, l.FirstName, l.LastName, l.Email, l.Phone, l.Company, l.Status, l.Source, l.EstimatedValue, l.AssignedToUserId, l.Tags, l.FollowUpDate, l.ConvertedOpportunityId, l.CreatedAt)));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLeadDto dto)
    {
        var lead = new Lead
        {
            TenantId = _tenant.TenantId,
            Title = dto.Title,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            Company = dto.Company,
            JobTitle = dto.JobTitle,
            Status = dto.Status,
            Source = dto.Source,
            EstimatedValue = dto.EstimatedValue,
            AssignedToUserId = dto.AssignedToUserId,
            Notes = dto.Notes,
            Tags = dto.Tags,
            FollowUpDate = dto.FollowUpDate,
            ContactId = dto.ContactId
        };
        _db.Leads.Add(lead);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = lead.Id },
            ApiResponse<LeadResponseDto>.Ok(new LeadResponseDto(lead.Id, lead.Title, lead.FirstName, lead.LastName, lead.Email, lead.Phone, lead.Company, lead.Status, lead.Source, lead.EstimatedValue, lead.AssignedToUserId, lead.Tags, lead.FollowUpDate, lead.ConvertedOpportunityId, lead.CreatedAt)));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLeadDto dto)
    {
        var lead = await _db.Leads.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (lead == null) return NotFound(ApiResponse<object>.Fail("Lead not found"));

        lead.Title = dto.Title;
        lead.FirstName = dto.FirstName;
        lead.LastName = dto.LastName;
        lead.Email = dto.Email;
        lead.Phone = dto.Phone;
        lead.Company = dto.Company;
        lead.JobTitle = dto.JobTitle;
        lead.Status = dto.Status;
        lead.Source = dto.Source;
        lead.EstimatedValue = dto.EstimatedValue;
        lead.AssignedToUserId = dto.AssignedToUserId;
        lead.Notes = dto.Notes;
        lead.Tags = dto.Tags;
        lead.FollowUpDate = dto.FollowUpDate;

        await _db.SaveChangesAsync();
        return Ok(ApiResponse<LeadResponseDto>.Ok(new LeadResponseDto(lead.Id, lead.Title, lead.FirstName, lead.LastName, lead.Email, lead.Phone, lead.Company, lead.Status, lead.Source, lead.EstimatedValue, lead.AssignedToUserId, lead.Tags, lead.FollowUpDate, lead.ConvertedOpportunityId, lead.CreatedAt)));
    }

    [HttpPost("{id:guid}/convert")]
    public async Task<IActionResult> Convert(Guid id, [FromBody] ConvertLeadDto dto)
    {
        var lead = await _db.Leads.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (lead == null) return NotFound(ApiResponse<object>.Fail("Lead not found"));
        if (lead.ConvertedOpportunityId.HasValue)
            return BadRequest(ApiResponse<object>.Fail("Lead already converted"));

        var opportunity = new Opportunity
        {
            TenantId = _tenant.TenantId,
            Title = dto.OpportunityTitle,
            LeadId = lead.Id,
            ContactId = lead.ContactId,
            ExpectedValue = dto.ExpectedValue,
            ExpectedCloseDate = dto.ExpectedCloseDate,
            AssignedToUserId = lead.AssignedToUserId
        };
        _db.Opportunities.Add(opportunity);
        lead.Status = LeadStatusEnum.Converted;
        lead.ConvertedOpportunityId = opportunity.Id;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(new { opportunityId = opportunity.Id }, "Lead converted to opportunity"));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var lead = await _db.Leads.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (lead == null) return NotFound(ApiResponse<object>.Fail("Lead not found"));
        lead.IsDeleted = true;
        lead.DeletedAt = DateTime.UtcNow;
        lead.DeletedBy = _tenant.UserId;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(null!, "Lead deleted"));
    }
}
