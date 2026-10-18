using ErpCrm.Application.Common.Models;
using ErpCrm.Application.Identity.Dtos;
using ErpCrm.Domain.Entities.Identity;
using ErpCrm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpCrm.API.Controllers.v1.Identity;

[ApiController]
[Route("api/v1/roles")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly RoleManager<AppRole> _roleManager;
    private readonly UserManager<AppUser> _userManager;
    private readonly AppDbContext _db;

    public RolesController(RoleManager<AppRole> roleManager, UserManager<AppUser> userManager, AppDbContext db)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<RoleResponseDto>>>> GetAll()
    {
        var roles = await _roleManager.Roles.ToListAsync();
        var dtos = new List<RoleResponseDto>();
        foreach (var role in roles)
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
            var permissions = await _db.RolePermissions
                .Where(rp => rp.RoleId == role.Id)
                .Include(rp => rp.Permission)
                .Select(rp => rp.Permission.Code)
                .ToListAsync();

            dtos.Add(new RoleResponseDto
            {
                Id = role.Id, Name = role.Name!, Description = role.Description,
                UserCount = usersInRole.Count, Permissions = permissions
            });
        }
        return Ok(ApiResponse<IEnumerable<RoleResponseDto>>.Ok(dtos));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<RoleResponseDto>>> Create([FromBody] CreateRoleDto dto)
    {
        if (await _roleManager.RoleExistsAsync(dto.Name))
            return BadRequest(ApiResponse<RoleResponseDto>.Fail("Role already exists"));

        var role = new AppRole { Name = dto.Name, Description = dto.Description };
        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
            return BadRequest(ApiResponse<RoleResponseDto>.Fail(string.Join(", ", result.Errors.Select(e => e.Description))));

        return Ok(ApiResponse<RoleResponseDto>.Ok(new RoleResponseDto { Id = role.Id, Name = role.Name! }));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<string>>> Update(Guid id, [FromBody] UpdateRoleDto dto)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString())
            ?? throw new KeyNotFoundException("Role not found");
        role.Name = dto.Name; role.Description = dto.Description;
        await _roleManager.UpdateAsync(role);
        return Ok(ApiResponse<string>.Ok("Role updated"));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(Guid id)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString())
            ?? throw new KeyNotFoundException("Role not found");
        if (role.IsSystemRole)
            return BadRequest(ApiResponse<string>.Fail("System roles cannot be deleted"));
        var result = await _roleManager.DeleteAsync(role);
        if (!result.Succeeded)
            return BadRequest(ApiResponse<string>.Fail("Cannot delete role"));
        return Ok(ApiResponse<string>.Ok("Role deleted"));
    }

    [HttpGet("permissions")]
    public async Task<ActionResult<ApiResponse<IEnumerable<object>>>> GetAllPermissions()
    {
        var permissions = await _db.Permissions
            .OrderBy(p => p.Module).ThenBy(p => p.Code)
            .Select(p => new { p.Id, p.Code, p.Name, p.Module })
            .ToListAsync();
        return Ok(ApiResponse<IEnumerable<object>>.Ok(permissions));
    }

    [HttpPost("{id:guid}/permissions")]
    public async Task<ActionResult<ApiResponse<string>>> AssignPermissions(Guid id, [FromBody] AssignPermissionsDto dto)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString())
            ?? throw new KeyNotFoundException("Role not found");

        var existing = _db.RolePermissions.Where(rp => rp.RoleId == id);
        _db.RolePermissions.RemoveRange(existing);

        var permIds = await _db.Permissions
            .Where(p => dto.Permissions.Contains(p.Code))
            .Select(p => p.Id).ToListAsync();
        foreach (var permId in permIds)
            _db.RolePermissions.Add(new RolePermission { RoleId = id, PermissionId = permId });

        await _db.SaveChangesAsync();
        return Ok(ApiResponse<string>.Ok("Permissions updated"));
    }
}
