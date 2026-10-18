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
[Route("api/v1/hr/employees")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;

    public EmployeesController(AppDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<EmployeeResponseDto>>>> GetAll([FromQuery] QueryParams q, [FromQuery] Guid? departmentId = null, [FromQuery] EmployeeStatus? status = null)
    {
        var query = _db.Employees.AsNoTracking().Where(e => !e.IsDeleted);
        if (departmentId.HasValue) query = query.Where(e => e.DepartmentId == departmentId);
        if (status.HasValue) query = query.Where(e => e.Status == status);
        if (!string.IsNullOrWhiteSpace(q.Search))
            query = query.Where(e => e.FirstName.Contains(q.Search) || e.LastName.Contains(q.Search) || e.EmployeeCode.Contains(q.Search));

        var total = await query.CountAsync();
        var items = await query.Include(e => e.Department).Include(e => e.Designation)
            .OrderBy(e => e.FirstName)
            .Skip((q.Page - 1) * q.PageSize).Take(q.PageSize)
            .Select(e => new EmployeeResponseDto
            {
                Id = e.Id, CompanyId = e.CompanyId, EmployeeCode = e.EmployeeCode,
                FirstName = e.FirstName, LastName = e.LastName,
                FullName = e.FirstName + " " + e.LastName,
                Email = e.Email, Phone = e.Phone,
                DateOfBirth = e.DateOfBirth, JoinDate = e.JoinDate,
                DepartmentId = e.DepartmentId, DepartmentName = e.Department != null ? e.Department.Name : null,
                DesignationId = e.DesignationId, DesignationName = e.Designation != null ? e.Designation.Name : null,
                ManagerId = e.ManagerId, Status = e.Status,
                BasicSalary = e.BasicSalary, AvatarUrl = e.AvatarUrl, CreatedAt = e.CreatedAt
            }).ToListAsync();

        var pagination = new PaginationMeta { Page = q.Page, PageSize = q.PageSize, TotalCount = total };
        return Ok(ApiResponse<PagedResult<EmployeeResponseDto>>.OkPaged(
            new PagedResult<EmployeeResponseDto> { Items = items, Pagination = pagination }, pagination));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<EmployeeResponseDto>>> GetById(Guid id)
    {
        var e = await _db.Employees.Include(e => e.Department).Include(e => e.Designation)
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted)
            ?? throw new KeyNotFoundException("Employee not found");

        return Ok(ApiResponse<EmployeeResponseDto>.Ok(new EmployeeResponseDto
        {
            Id = e.Id, CompanyId = e.CompanyId, EmployeeCode = e.EmployeeCode,
            FirstName = e.FirstName, LastName = e.LastName, FullName = e.FirstName + " " + e.LastName,
            Email = e.Email, Phone = e.Phone, DateOfBirth = e.DateOfBirth, JoinDate = e.JoinDate,
            DepartmentId = e.DepartmentId, DepartmentName = e.Department?.Name,
            DesignationId = e.DesignationId, DesignationName = e.Designation?.Name,
            ManagerId = e.ManagerId, Status = e.Status,
            BasicSalary = e.BasicSalary, AvatarUrl = e.AvatarUrl, CreatedAt = e.CreatedAt
        }));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<EmployeeResponseDto>>> Create([FromBody] CreateEmployeeDto dto)
    {
        var count = await _db.Employees.IgnoreQueryFilters()
            .CountAsync(e => e.TenantId == _tenantContext.TenantId);
        var code = $"EMP-{(count + 1):D4}";

        var employee = new Employee
        {
            TenantId = _tenantContext.TenantId, CompanyId = dto.CompanyId,
            EmployeeCode = code, FirstName = dto.FirstName, LastName = dto.LastName,
            Email = dto.Email, Phone = dto.Phone, DateOfBirth = dto.DateOfBirth,
            JoinDate = dto.JoinDate ?? DateTime.UtcNow, DepartmentId = dto.DepartmentId,
            DesignationId = dto.DesignationId, ManagerId = dto.ManagerId,
            BasicSalary = dto.BasicSalary, NationalId = dto.NationalId,
            BankAccount = dto.BankAccount, UserId = dto.UserId
        };
        _db.Employees.Add(employee);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = employee.Id }, ApiResponse<EmployeeResponseDto>.Ok(new EmployeeResponseDto
        {
            Id = employee.Id, CompanyId = employee.CompanyId, EmployeeCode = employee.EmployeeCode,
            FirstName = employee.FirstName, LastName = employee.LastName,
            FullName = employee.FirstName + " " + employee.LastName,
            Status = employee.Status, JoinDate = employee.JoinDate, CreatedAt = employee.CreatedAt
        }));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<string>>> Update(Guid id, [FromBody] UpdateEmployeeDto dto)
    {
        var employee = await _db.Employees.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted)
            ?? throw new KeyNotFoundException("Employee not found");
        employee.FirstName = dto.FirstName; employee.LastName = dto.LastName;
        employee.Email = dto.Email; employee.Phone = dto.Phone;
        employee.DateOfBirth = dto.DateOfBirth; employee.JoinDate = dto.JoinDate;
        employee.DepartmentId = dto.DepartmentId; employee.DesignationId = dto.DesignationId;
        employee.ManagerId = dto.ManagerId; employee.Status = dto.Status;
        employee.BasicSalary = dto.BasicSalary; employee.NationalId = dto.NationalId;
        employee.BankAccount = dto.BankAccount;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<string>.Ok("Employee updated"));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(Guid id)
    {
        var employee = await _db.Employees.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted)
            ?? throw new KeyNotFoundException("Employee not found");
        employee.IsDeleted = true; employee.DeletedAt = DateTime.UtcNow; employee.DeletedBy = _tenantContext.UserId;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<string>.Ok("Employee deleted"));
    }
}
