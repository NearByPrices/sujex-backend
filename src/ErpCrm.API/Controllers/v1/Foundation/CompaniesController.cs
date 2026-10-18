using ErpCrm.Application.Common.Interfaces;
using ErpCrm.Application.Common.Models;
using ErpCrm.Application.Foundation.Dtos;
using ErpCrm.Domain.Entities.Foundation;
using ErpCrm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpCrm.API.Controllers.v1.Foundation;

[ApiController]
[Route("api/v1/companies")]
[Authorize]
public class CompaniesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;

    public CompaniesController(AppDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<CompanyResponseDto>>>> GetAll()
    {
        var companies = await _db.Companies.AsNoTracking()
            .Select(c => new CompanyResponseDto
            {
                Id = c.Id,
                TenantId = c.TenantId,
                Name = c.Name,
                LogoUrl = c.LogoUrl,
                TaxId = c.TaxId,
                Website = c.Website,
                Phone = c.Phone,
                Email = c.Email,
                AddressLine1 = c.AddressLine1,
                City = c.City,
                Country = c.Country,
                BaseCurrency = c.BaseCurrency,
                Timezone = c.Timezone,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();
        return Ok(ApiResponse<IEnumerable<CompanyResponseDto>>.Ok(companies));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CompanyResponseDto>>> GetById(Guid id)
    {
        var c = await _db.Companies.FindAsync(id)
            ?? throw new KeyNotFoundException("Company not found");
        return Ok(ApiResponse<CompanyResponseDto>.Ok(MapToResponse(c)));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CompanyResponseDto>>> Create([FromBody] CreateCompanyDto dto)
    {
        var company = new Company
        {
            TenantId = _tenantContext.TenantId,
            Name = dto.Name,
            BaseCurrency = dto.BaseCurrency,
            Timezone = dto.Timezone,
            TaxId = dto.TaxId,
            Phone = dto.Phone,
            Email = dto.Email,
            AddressLine1 = dto.AddressLine1,
            City = dto.City,
            Country = dto.Country
        };
        _db.Companies.Add(company);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = company.Id },
            ApiResponse<CompanyResponseDto>.Ok(MapToResponse(company)));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<CompanyResponseDto>>> Update(
        Guid id, [FromBody] UpdateCompanyDto dto)
    {
        var company = await _db.Companies.FindAsync(id)
            ?? throw new KeyNotFoundException("Company not found");

        company.Name = dto.Name;
        company.TaxId = dto.TaxId;
        company.Website = dto.Website;
        company.Phone = dto.Phone;
        company.Email = dto.Email;
        company.AddressLine1 = dto.AddressLine1;
        company.AddressLine2 = dto.AddressLine2;
        company.City = dto.City;
        company.State = dto.State;
        company.Country = dto.Country;
        company.PostalCode = dto.PostalCode;
        company.BaseCurrency = dto.BaseCurrency;
        company.Timezone = dto.Timezone;
        company.DateFormat = dto.DateFormat;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<CompanyResponseDto>.Ok(MapToResponse(company)));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(Guid id)
    {
        var company = await _db.Companies.FindAsync(id)
            ?? throw new KeyNotFoundException("Company not found");
        _db.Companies.Remove(company);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<string>.Ok("Company deleted"));
    }

    private static CompanyResponseDto MapToResponse(Company c) => new()
    {
        Id = c.Id,
        TenantId = c.TenantId,
        Name = c.Name,
        LogoUrl = c.LogoUrl,
        TaxId = c.TaxId,
        Website = c.Website,
        Phone = c.Phone,
        Email = c.Email,
        AddressLine1 = c.AddressLine1,
        City = c.City,
        Country = c.Country,
        BaseCurrency = c.BaseCurrency,
        Timezone = c.Timezone,
        IsActive = c.IsActive,
        CreatedAt = c.CreatedAt
    };
}
