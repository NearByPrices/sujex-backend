using ErpCrm.Application.Common.Models;
using ErpCrm.Application.Foundation.Dtos;
using ErpCrm.Domain.Entities.Foundation;
using ErpCrm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpCrm.API.Controllers.v1.Foundation;

[ApiController]
[Route("api/v1/companies/{companyId:guid}/branches")]
[Authorize]
public class BranchesController : ControllerBase
{
    private readonly AppDbContext _db;

    public BranchesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<BranchResponseDto>>>> GetAll(Guid companyId)
    {
        var branches = await _db.Branches
            .Where(b => b.CompanyId == companyId)
            .Select(b => new BranchResponseDto
            {
                Id = b.Id,
                CompanyId = b.CompanyId,
                Name = b.Name,
                Code = b.Code,
                Phone = b.Phone,
                Email = b.Email,
                AddressLine1 = b.AddressLine1,
                City = b.City,
                Country = b.Country,
                IsActive = b.IsActive,
                IsHeadOffice = b.IsHeadOffice
            })
            .ToListAsync();
        return Ok(ApiResponse<IEnumerable<BranchResponseDto>>.Ok(branches));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<BranchResponseDto>>> GetById(Guid companyId, Guid id)
    {
        var b = await _db.Branches
            .FirstOrDefaultAsync(b => b.Id == id && b.CompanyId == companyId)
            ?? throw new KeyNotFoundException("Branch not found");
        return Ok(ApiResponse<BranchResponseDto>.Ok(MapToResponse(b)));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<BranchResponseDto>>> Create(
        Guid companyId, [FromBody] CreateBranchDto dto)
    {
        var branch = new Branch
        {
            CompanyId = companyId,
            Name = dto.Name,
            Code = dto.Code,
            Phone = dto.Phone,
            Email = dto.Email,
            AddressLine1 = dto.AddressLine1,
            City = dto.City,
            Country = dto.Country,
            IsHeadOffice = dto.IsHeadOffice
        };
        _db.Branches.Add(branch);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<BranchResponseDto>.Ok(MapToResponse(branch)));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<string>>> Update(
        Guid companyId, Guid id, [FromBody] UpdateBranchDto dto)
    {
        var branch = await _db.Branches
            .FirstOrDefaultAsync(b => b.Id == id && b.CompanyId == companyId)
            ?? throw new KeyNotFoundException("Branch not found");

        branch.Name = dto.Name;
        branch.Code = dto.Code;
        branch.Phone = dto.Phone;
        branch.Email = dto.Email;
        branch.AddressLine1 = dto.AddressLine1;
        branch.City = dto.City;
        branch.Country = dto.Country;
        branch.IsHeadOffice = dto.IsHeadOffice;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<string>.Ok("Branch updated"));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(Guid companyId, Guid id)
    {
        var branch = await _db.Branches
            .FirstOrDefaultAsync(b => b.Id == id && b.CompanyId == companyId)
            ?? throw new KeyNotFoundException("Branch not found");
        _db.Branches.Remove(branch);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<string>.Ok("Branch deleted"));
    }

    private static BranchResponseDto MapToResponse(Branch b) => new()
    {
        Id = b.Id,
        CompanyId = b.CompanyId,
        Name = b.Name,
        Code = b.Code,
        Phone = b.Phone,
        Email = b.Email,
        AddressLine1 = b.AddressLine1,
        City = b.City,
        Country = b.Country,
        IsActive = b.IsActive,
        IsHeadOffice = b.IsHeadOffice
    };
}
