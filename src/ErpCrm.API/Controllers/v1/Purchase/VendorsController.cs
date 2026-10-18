using ErpCrm.Application.Common.Models;
using ErpCrm.Application.Common.Interfaces;
using ErpCrm.Application.Purchase.Dtos;
using ErpCrm.Domain.Entities.Purchase;
using ErpCrm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpCrm.API.Controllers.v1.Purchase;

[ApiController]
[Route("api/v1/purchase/vendors")]
[Authorize]
public class VendorsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;

    public VendorsController(AppDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<VendorResponseDto>>>> GetAll([FromQuery] QueryParams q)
    {
        var query = _db.Vendors.AsNoTracking().Where(v => !v.IsDeleted);
        if (!string.IsNullOrWhiteSpace(q.Search))
            query = query.Where(v => v.Name.Contains(q.Search) || (v.Code != null && v.Code.Contains(q.Search)));

        var total = await query.CountAsync();
        var items = await query.OrderBy(v => v.Name)
            .Skip((q.Page - 1) * q.PageSize).Take(q.PageSize)
            .Select(v => new VendorResponseDto
            {
                Id = v.Id, Name = v.Name, Code = v.Code, Email = v.Email, Phone = v.Phone,
                Address = v.Address, TaxNumber = v.TaxNumber, CurrencyCode = v.CurrencyCode,
                PaymentTermsDays = v.PaymentTermsDays, IsActive = v.IsActive,
                Notes = v.Notes, CreatedAt = v.CreatedAt
            }).ToListAsync();

        var pagination = new PaginationMeta { Page = q.Page, PageSize = q.PageSize, TotalCount = total };
        return Ok(ApiResponse<PagedResult<VendorResponseDto>>.OkPaged(
            new PagedResult<VendorResponseDto> { Items = items, Pagination = pagination }, pagination));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<VendorResponseDto>>> GetById(Guid id)
    {
        var v = await _db.Vendors.FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted)
            ?? throw new KeyNotFoundException("Vendor not found");
        return Ok(ApiResponse<VendorResponseDto>.Ok(Map(v)));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<VendorResponseDto>>> Create([FromBody] CreateVendorDto dto)
    {
        var vendor = new Vendor
        {
            TenantId = _tenantContext.TenantId, Name = dto.Name, Code = dto.Code,
            Email = dto.Email, Phone = dto.Phone, Address = dto.Address,
            TaxNumber = dto.TaxNumber, CurrencyCode = dto.CurrencyCode,
            PaymentTermsDays = dto.PaymentTermsDays, Notes = dto.Notes
        };
        _db.Vendors.Add(vendor);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = vendor.Id }, ApiResponse<VendorResponseDto>.Ok(Map(vendor)));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<VendorResponseDto>>> Update(Guid id, [FromBody] UpdateVendorDto dto)
    {
        var vendor = await _db.Vendors.FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted)
            ?? throw new KeyNotFoundException("Vendor not found");
        vendor.Name = dto.Name; vendor.Code = dto.Code; vendor.Email = dto.Email;
        vendor.Phone = dto.Phone; vendor.Address = dto.Address; vendor.TaxNumber = dto.TaxNumber;
        vendor.CurrencyCode = dto.CurrencyCode; vendor.PaymentTermsDays = dto.PaymentTermsDays;
        vendor.IsActive = dto.IsActive; vendor.Notes = dto.Notes;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<VendorResponseDto>.Ok(Map(vendor)));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(Guid id)
    {
        var vendor = await _db.Vendors.FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted)
            ?? throw new KeyNotFoundException("Vendor not found");
        vendor.IsDeleted = true; vendor.DeletedAt = DateTime.UtcNow; vendor.DeletedBy = _tenantContext.UserId;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<string>.Ok("Vendor deleted"));
    }

    private static VendorResponseDto Map(Vendor v) => new()
    {
        Id = v.Id, Name = v.Name, Code = v.Code, Email = v.Email, Phone = v.Phone,
        Address = v.Address, TaxNumber = v.TaxNumber, CurrencyCode = v.CurrencyCode,
        PaymentTermsDays = v.PaymentTermsDays, IsActive = v.IsActive,
        Notes = v.Notes, CreatedAt = v.CreatedAt
    };
}
