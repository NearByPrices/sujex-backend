using ErpCrm.Application.Common.Models;
using ErpCrm.Application.Common.Interfaces;
using ErpCrm.Application.HR.Dtos;
using ErpCrm.Domain.Entities.HR;
using ErpCrm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpCrm.API.Controllers.v1.HR;

[ApiController]
[Route("api/v1/hr/departments")]
[Authorize]
public class DepartmentsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;

    public DepartmentsController(AppDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<DepartmentResponseDto>>>> GetAll([FromQuery] Guid? companyId = null)
    {
        var query = _db.Departments.AsNoTracking();
        if (companyId.HasValue) query = query.Where(d => d.CompanyId == companyId);

        var departments = await query
            .Select(d => new DepartmentResponseDto
            {
                Id = d.Id, CompanyId = d.CompanyId ?? Guid.Empty, Name = d.Name, Code = d.Code,
                ManagerId = null, IsActive = true,
                EmployeeCount = d.Employees.Count(e => e.Status == ErpCrm.Domain.Enums.EmployeeStatus.Active)
            }).ToListAsync();
        return Ok(ApiResponse<IEnumerable<DepartmentResponseDto>>.Ok(departments));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<DepartmentResponseDto>>> Create([FromBody] CreateDepartmentDto dto)
    {
        var dept = new Department { TenantId = _tenantContext.TenantId, CompanyId = dto.CompanyId, Name = dto.Name, Code = dto.Code };
        _db.Departments.Add(dept);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<DepartmentResponseDto>.Ok(new DepartmentResponseDto { Id = dept.Id, CompanyId = dept.CompanyId ?? Guid.Empty, Name = dept.Name, Code = dept.Code, IsActive = true }));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<string>>> Update(Guid id, [FromBody] UpdateDepartmentDto dto)
    {
        var dept = await _db.Departments.FindAsync(id) ?? throw new KeyNotFoundException("Department not found");
        dept.Name = dto.Name; dept.Code = dto.Code;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<string>.Ok("Department updated"));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(Guid id)
    {
        var dept = await _db.Departments.FindAsync(id) ?? throw new KeyNotFoundException("Department not found");
        _db.Departments.Remove(dept);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<string>.Ok("Department deleted"));
    }
}
