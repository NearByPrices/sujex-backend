using ErpCrm.Application.Common.Interfaces;
using ErpCrm.Application.Common.Models;
using ErpCrm.Application.Inventory.Dtos;
using ErpCrm.Domain.Entities.Inventory;
using ErpCrm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpCrm.API.Controllers.v1.Inventory;

[Authorize]
[ApiController]
[Route("api/v1/products")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenant;

    public ProductsController(AppDbContext db, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null, [FromQuery] Guid? categoryId = null)
    {
        var query = _db.Products.Include(p => p.Category).Include(p => p.Unit).Where(p => !p.IsDeleted);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.Contains(search) || p.Code!.Contains(search) || p.Sku!.Contains(search));
        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId);

        var total = await query.CountAsync();
        var items = await query.OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(p => new ProductResponseDto(p.Id, p.Name, p.Code, p.Sku, p.Barcode, p.Description, p.CategoryId, p.Category != null ? p.Category.Name : null, p.UnitId, p.Unit != null ? p.Unit.Symbol : null, p.CostPrice, p.SalePrice, p.IsActive, p.IsService, p.Tags, p.CreatedAt))
            .ToListAsync();

        return Ok(ApiResponse<List<ProductResponseDto>>.OkPaged(items, new PaginationMeta { Page = page, PageSize = pageSize, TotalCount = total }));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var p = await _db.Products.Include(x => x.Category).Include(x => x.Unit).FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (p == null) return NotFound(ApiResponse<object>.Fail("Product not found"));
        return Ok(ApiResponse<ProductResponseDto>.Ok(new ProductResponseDto(p.Id, p.Name, p.Code, p.Sku, p.Barcode, p.Description, p.CategoryId, p.Category?.Name, p.UnitId, p.Unit?.Symbol, p.CostPrice, p.SalePrice, p.IsActive, p.IsService, p.Tags, p.CreatedAt)));
    }

    [HttpGet("{id:guid}/stock")]
    public async Task<IActionResult> GetStock(Guid id)
    {
        var items = await _db.InventoryItems.Include(i => i.Warehouse)
            .Where(i => i.ProductId == id)
            .Select(i => new StockLevelDto(i.ProductId, "", i.WarehouseId, i.Warehouse.Name, i.QuantityOnHand, i.QuantityReserved, i.QuantityAvailable))
            .ToListAsync();
        return Ok(ApiResponse<List<StockLevelDto>>.Ok(items));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        var product = new Product
        {
            TenantId = _tenant.TenantId,
            Name = dto.Name,
            Code = dto.Code,
            Sku = dto.Sku,
            Barcode = dto.Barcode,
            Description = dto.Description,
            CategoryId = dto.CategoryId,
            UnitId = dto.UnitId,
            CostPrice = dto.CostPrice,
            SalePrice = dto.SalePrice,
            MinStockLevel = dto.MinStockLevel,
            MaxStockLevel = dto.MaxStockLevel,
            ReorderPoint = dto.ReorderPoint,
            IsService = dto.IsService,
            Tags = dto.Tags
        };
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = product.Id },
            ApiResponse<ProductResponseDto>.Ok(new ProductResponseDto(product.Id, product.Name, product.Code, product.Sku, product.Barcode, product.Description, product.CategoryId, null, product.UnitId, null, product.CostPrice, product.SalePrice, product.IsActive, product.IsService, product.Tags, product.CreatedAt)));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductDto dto)
    {
        var product = await _db.Products.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (product == null) return NotFound(ApiResponse<object>.Fail("Product not found"));

        product.Name = dto.Name;
        product.Code = dto.Code;
        product.Sku = dto.Sku;
        product.Barcode = dto.Barcode;
        product.Description = dto.Description;
        product.CategoryId = dto.CategoryId;
        product.UnitId = dto.UnitId;
        product.CostPrice = dto.CostPrice;
        product.SalePrice = dto.SalePrice;
        product.MinStockLevel = dto.MinStockLevel;
        product.MaxStockLevel = dto.MaxStockLevel;
        product.ReorderPoint = dto.ReorderPoint;
        product.IsActive = dto.IsActive;
        product.IsService = dto.IsService;
        product.Tags = dto.Tags;

        await _db.SaveChangesAsync();
        return Ok(ApiResponse<ProductResponseDto>.Ok(new ProductResponseDto(product.Id, product.Name, product.Code, product.Sku, product.Barcode, product.Description, product.CategoryId, null, product.UnitId, null, product.CostPrice, product.SalePrice, product.IsActive, product.IsService, product.Tags, product.CreatedAt)));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var product = await _db.Products.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (product == null) return NotFound(ApiResponse<object>.Fail("Product not found"));
        product.IsDeleted = true;
        product.DeletedAt = DateTime.UtcNow;
        product.DeletedBy = _tenant.UserId;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(null!, "Product deleted"));
    }
}
