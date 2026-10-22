using ErpCrm.Application.Common.Models;
using ErpCrm.Application.Tenants.Dtos;
using ErpCrm.Domain.Entities.Platform;
using ErpCrm.Domain.Enums;
using ErpCrm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpCrm.API.Controllers.Platform;

[ApiController]
[Route("api/platform/tenants")]
[Authorize(Policy = "PlatformAdmin")]
public class TenantsController : ControllerBase
{
    private readonly AppDbContext _db;

    public TenantsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<TenantListDto>>>> GetAll([FromQuery] QueryParams q)
    {
        var query = _db.Tenants.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q.Search))
            query = query.Where(t => t.Name.Contains(q.Search) || t.Slug.Contains(q.Search));

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .Select(t => new TenantListDto
            {
                Id = t.Id,
                Name = t.Name,
                Slug = t.Slug,
                Status = t.Status,
                ContactEmail = t.ContactEmail,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();

        var pagination = new PaginationMeta { Page = q.Page, PageSize = q.PageSize, TotalCount = total };
        return Ok(ApiResponse<PagedResult<TenantListDto>>.OkPaged(
            new PagedResult<TenantListDto> { Items = items, Pagination = pagination },
            pagination));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<TenantResponseDto>>> GetById(Guid id)
    {
        var t = await _db.Tenants.FindAsync(id)
            ?? throw new KeyNotFoundException("Tenant not found");
        return Ok(ApiResponse<TenantResponseDto>.Ok(MapToResponse(t)));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<TenantResponseDto>>> Create([FromBody] CreateTenantDto dto)
    {
        if (await _db.Tenants.AnyAsync(t => t.Slug == dto.Slug))
            return BadRequest(ApiResponse<TenantResponseDto>.Fail("Slug already in use"));

        var tenant = new Tenant
        {
            Name = dto.Name,
            Slug = dto.Slug,
            ContactEmail = dto.ContactEmail,
            ContactPhone = dto.ContactPhone,
            MaxUsers = dto.MaxUsers,
            MaxCompanies = dto.MaxCompanies,
            Status = TenantStatus.Active
        };
        _db.Tenants.Add(tenant);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = tenant.Id },
            ApiResponse<TenantResponseDto>.Ok(MapToResponse(tenant)));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<TenantResponseDto>>> Update(Guid id, [FromBody] UpdateTenantDto dto)
    {
        var tenant = await _db.Tenants.FindAsync(id)
            ?? throw new KeyNotFoundException("Tenant not found");

        tenant.Name = dto.Name;
        tenant.ContactEmail = dto.ContactEmail;
        tenant.ContactPhone = dto.ContactPhone;
        tenant.MaxUsers = dto.MaxUsers;
        tenant.MaxCompanies = dto.MaxCompanies;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<TenantResponseDto>.Ok(MapToResponse(tenant)));
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<ApiResponse<string>>> UpdateStatus(Guid id, [FromBody] TenantStatus status)
    {
        var tenant = await _db.Tenants.FindAsync(id)
            ?? throw new KeyNotFoundException("Tenant not found");
        tenant.Status = status;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<string>.Ok("Status updated"));
    }

    [HttpGet("{id:guid}/modules")]
    public async Task<ActionResult<ApiResponse<IEnumerable<object>>>> GetModules(Guid id)
    {
        var modules = await _db.TenantModules
            .Where(m => m.TenantId == id)
            .Select(m => new { m.ModuleCode, m.IsEnabled })
            .ToListAsync();
        return Ok(ApiResponse<IEnumerable<object>>.Ok(modules.Cast<object>()));
    }

    [HttpPut("{id:guid}/modules")]
    public async Task<ActionResult<ApiResponse<string>>> UpdateModules(
        Guid id, [FromBody] List<ModuleCode> enabledModules)
    {
        var existing = await _db.TenantModules.Where(m => m.TenantId == id).ToListAsync();
        _db.TenantModules.RemoveRange(existing);
        foreach (var code in enabledModules)
        {
            _db.TenantModules.Add(new TenantModule { TenantId = id, ModuleCode = code, IsEnabled = true });
        }
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<string>.Ok("Modules updated"));
    }

    private static TenantResponseDto MapToResponse(Tenant t) => new()
    {
        Id = t.Id,
        Name = t.Name,
        Slug = t.Slug,
        Status = t.Status,
        ContactEmail = t.ContactEmail,
        ContactPhone = t.ContactPhone,
        LogoUrl = t.LogoUrl,
        MaxUsers = t.MaxUsers,
        MaxCompanies = t.MaxCompanies,
        CreatedAt = t.CreatedAt
    };
}
