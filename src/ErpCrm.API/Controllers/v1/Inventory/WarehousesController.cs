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
[Route("api/v1/warehouses")]
public class WarehousesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenant;

    public WarehousesController(AppDbContext db, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _db.Warehouses
            .Select(w => new WarehouseResponseDto(w.Id, w.Name, w.Code, w.BranchId, w.Address, w.IsActive, w.CreatedAt))
            .ToListAsync();
        return Ok(ApiResponse<List<WarehouseResponseDto>>.Ok(items));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var w = await _db.Warehouses.FirstOrDefaultAsync(x => x.Id == id);
        if (w == null) return NotFound(ApiResponse<object>.Fail("Warehouse not found"));
        return Ok(ApiResponse<WarehouseResponseDto>.Ok(new WarehouseResponseDto(w.Id, w.Name, w.Code, w.BranchId, w.Address, w.IsActive, w.CreatedAt)));
    }

    [HttpGet("{id:guid}/stock")]
    public async Task<IActionResult> GetStock(Guid id)
    {
        var items = await _db.InventoryItems.Include(i => i.Product)
            .Where(i => i.WarehouseId == id)
            .Select(i => new StockLevelDto(i.ProductId, i.Product.Name, i.WarehouseId, "", i.QuantityOnHand, i.QuantityReserved, i.QuantityAvailable))
            .ToListAsync();
        return Ok(ApiResponse<List<StockLevelDto>>.Ok(items));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWarehouseDto dto)
    {
        var warehouse = new Warehouse
        {
            TenantId = _tenant.TenantId,
            Name = dto.Name,
            Code = dto.Code,
            BranchId = dto.BranchId,
            Address = dto.Address
        };
        _db.Warehouses.Add(warehouse);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = warehouse.Id },
            ApiResponse<WarehouseResponseDto>.Ok(new WarehouseResponseDto(warehouse.Id, warehouse.Name, warehouse.Code, warehouse.BranchId, warehouse.Address, warehouse.IsActive, warehouse.CreatedAt)));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWarehouseDto dto)
    {
        var w = await _db.Warehouses.FirstOrDefaultAsync(x => x.Id == id);
        if (w == null) return NotFound(ApiResponse<object>.Fail("Warehouse not found"));
        w.Name = dto.Name;
        w.Code = dto.Code;
        w.BranchId = dto.BranchId;
        w.Address = dto.Address;
        w.IsActive = dto.IsActive;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<WarehouseResponseDto>.Ok(new WarehouseResponseDto(w.Id, w.Name, w.Code, w.BranchId, w.Address, w.IsActive, w.CreatedAt)));
    }
}
