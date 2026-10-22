using ErpCrm.Application.Common.Interfaces;
using ErpCrm.Application.Common.Models;
using ErpCrm.Domain.Entities.Inventory;
using ErpCrm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpCrm.API.Controllers.v1.Inventory;

[ApiController]
[Route("api/v1/inventory/categories")]
[Authorize]
public class ProductCategoriesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenant;

    public ProductCategoriesController(AppDbContext db, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<object>>>> GetAll()
    {
        var categories = await _db.ProductCategories.AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new { c.Id, c.Name, c.Code, c.Description, c.ParentId, c.CreatedAt })
            .ToListAsync();
        return Ok(ApiResponse<IEnumerable<object>>.Ok(categories.Cast<object>()));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<object>>> Create([FromBody] CreateCategoryRequest request)
    {
        if (await _db.ProductCategories.AnyAsync(c => c.Code == request.Code && request.Code != null))
            return BadRequest(ApiResponse<object>.Fail("Category code already exists"));

        var category = new ProductCategory
        {
            TenantId = _tenant.TenantId,
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            ParentId = request.ParentId
        };
        _db.ProductCategories.Add(category);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(new
        {
            category.Id, category.Name, category.Code, category.Description,
            category.ParentId, category.CreatedAt
        }));
    }

    public sealed record CreateCategoryRequest(
        string Name,
        string? Code,
        string? Description,
        Guid? ParentId = null);
}
