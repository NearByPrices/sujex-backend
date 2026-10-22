using ErpCrm.Application.Common.Models;
using ErpCrm.Application.Common.Interfaces;
using ErpCrm.Application.Identity.Dtos;
using ErpCrm.Domain.Entities.Identity;
using ErpCrm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpCrm.API.Controllers.v1.Identity;

[ApiController]
[Route("api/v1/users")]
[Authorize(Policy = "TenantAdmin")]
public class UsersController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;

    public UsersController(UserManager<AppUser> userManager, AppDbContext db, ITenantContext tenantContext)
    {
        _userManager = userManager;
        _db = db;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<UserResponseDto>>>> GetAll([FromQuery] QueryParams q)
    {
        var query = _userManager.Users.Where(u => u.TenantId == _tenantContext.TenantId);
        if (!string.IsNullOrWhiteSpace(q.Search))
            query = query.Where(u => u.Email!.Contains(q.Search) || u.FirstName.Contains(q.Search) || u.LastName.Contains(q.Search));

        var total = await query.CountAsync();
        var users = await query.OrderBy(u => u.FirstName)
            .Skip((q.Page - 1) * q.PageSize).Take(q.PageSize)
            .ToListAsync();

        var dtos = new List<UserResponseDto>();
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            dtos.Add(new UserResponseDto
            {
                Id = u.Id, Email = u.Email ?? string.Empty,
                FirstName = u.FirstName, LastName = u.LastName,
                FullName = u.FullName, Phone = u.PhoneNumber,
                IsActive = u.IsActive, EmailConfirmed = u.EmailConfirmed,
                Roles = roles, CreatedAt = u.CreatedAt
            });
        }

        var pagination = new PaginationMeta { Page = q.Page, PageSize = q.PageSize, TotalCount = total };
        return Ok(ApiResponse<PagedResult<UserResponseDto>>.OkPaged(
            new PagedResult<UserResponseDto> { Items = dtos, Pagination = pagination }, pagination));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UserResponseDto>>> GetById(Guid id)
    {
        var u = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id && u.TenantId == _tenantContext.TenantId)
            ?? throw new KeyNotFoundException("User not found");
        var roles = await _userManager.GetRolesAsync(u);
        return Ok(ApiResponse<UserResponseDto>.Ok(new UserResponseDto
        {
            Id = u.Id, Email = u.Email ?? string.Empty,
            FirstName = u.FirstName, LastName = u.LastName,
            FullName = u.FullName, Phone = u.PhoneNumber,
            IsActive = u.IsActive, EmailConfirmed = u.EmailConfirmed,
            Roles = roles, CreatedAt = u.CreatedAt
        }));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<UserResponseDto>>> Invite([FromBody] InviteUserDto dto)
    {
        if (await _userManager.FindByEmailAsync(dto.Email) != null)
            return BadRequest(ApiResponse<UserResponseDto>.Fail("Email already registered"));

        var user = new AppUser
        {
            UserName = dto.Email, Email = dto.Email,
            FirstName = dto.FirstName, LastName = dto.LastName,
            TenantId = _tenantContext.TenantId, EmailConfirmed = true
        };
        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest(ApiResponse<UserResponseDto>.Fail(string.Join(", ", result.Errors.Select(e => e.Description))));

        if (!string.IsNullOrWhiteSpace(dto.Role))
        {
            var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == dto.Role &&
                (r.TenantId == _tenantContext.TenantId || r.IsSystemRole));
            if (role == null)
                return BadRequest(ApiResponse<UserResponseDto>.Fail("Role is not available for this tenant"));
            await _userManager.AddToRoleAsync(user, dto.Role);
        }

        return Ok(ApiResponse<UserResponseDto>.Ok(new UserResponseDto
        {
            Id = user.Id, Email = user.Email!, FirstName = user.FirstName,
            LastName = user.LastName, FullName = user.FullName,
            IsActive = user.IsActive, Roles = new List<string> { dto.Role }
        }));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<string>>> Update(Guid id, [FromBody] UpdateUserDto dto)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id && u.TenantId == _tenantContext.TenantId)
            ?? throw new KeyNotFoundException("User not found");
        user.FirstName = dto.FirstName; user.LastName = dto.LastName;
        user.PhoneNumber = dto.Phone; user.IsActive = dto.IsActive;
        await _userManager.UpdateAsync(user);
        return Ok(ApiResponse<string>.Ok("User updated"));
    }

    [HttpPatch("{id:guid}/role")]
    public async Task<ActionResult<ApiResponse<string>>> ChangeRole(Guid id, [FromBody] ChangeUserRoleDto dto)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id && u.TenantId == _tenantContext.TenantId)
            ?? throw new KeyNotFoundException("User not found");
        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == dto.Role &&
            (r.TenantId == _tenantContext.TenantId || r.IsSystemRole));
        if (role == null)
            return BadRequest(ApiResponse<string>.Fail("Role is not available for this tenant"));
        await _userManager.AddToRoleAsync(user, dto.Role);
        return Ok(ApiResponse<string>.Ok("Role updated"));
    }

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<ActionResult<ApiResponse<string>>> Deactivate(Guid id)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id && u.TenantId == _tenantContext.TenantId)
            ?? throw new KeyNotFoundException("User not found");
        if (user.Id == _tenantContext.UserId)
            return BadRequest(ApiResponse<string>.Fail("Cannot deactivate yourself"));
        user.IsActive = false;
        await _userManager.UpdateAsync(user);
        return Ok(ApiResponse<string>.Ok("User deactivated"));
    }
}
