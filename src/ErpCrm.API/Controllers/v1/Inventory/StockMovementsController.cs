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
[Route("api/v1/stock-movements")]
public class StockMovementsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenant;

    public StockMovementsController(AppDbContext db, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] Guid? productId = null, [FromQuery] Guid? warehouseId = null)
    {
        var query = _db.StockMovements.Include(s => s.Product).Include(s => s.Warehouse).AsQueryable();
        if (productId.HasValue) query = query.Where(s => s.ProductId == productId);
        if (warehouseId.HasValue) query = query.Where(s => s.WarehouseId == warehouseId);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(s => s.MovementDate)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(s => new StockMovementResponseDto(s.Id, s.ProductId, s.Product.Name, s.WarehouseId, s.Warehouse.Name, s.MovementType, s.Quantity, s.UnitCost, s.TotalCost, s.Reference, s.Notes, s.MovementDate, s.CreatedAt))
            .ToListAsync();

        return Ok(ApiResponse<List<StockMovementResponseDto>>.OkPaged(items, new PaginationMeta { Page = page, PageSize = pageSize, TotalCount = total }));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStockMovementDto dto)
    {
        using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            var movement = new StockMovement
            {
                TenantId = _tenant.TenantId,
                ProductId = dto.ProductId,
                WarehouseId = dto.WarehouseId,
                MovementType = dto.MovementType,
                Quantity = dto.Quantity,
                UnitCost = dto.UnitCost,
                Reference = dto.Reference,
                Notes = dto.Notes,
                MovementDate = dto.MovementDate ?? DateTime.UtcNow
            };
            _db.StockMovements.Add(movement);

            var item = await _db.InventoryItems.FirstOrDefaultAsync(i => i.ProductId == dto.ProductId && i.WarehouseId == dto.WarehouseId);
            if (item == null)
            {
                item = new InventoryItem { TenantId = _tenant.TenantId, ProductId = dto.ProductId, WarehouseId = dto.WarehouseId };
                _db.InventoryItems.Add(item);
            }

            if (dto.MovementType == "IN")
                item.QuantityOnHand += dto.Quantity;
            else if (dto.MovementType == "OUT")
            {
                if (item.QuantityAvailable < dto.Quantity)
                {
                    await tx.RollbackAsync();
                    return BadRequest(ApiResponse<object>.Fail("Insufficient stock"));
                }
                item.QuantityOnHand -= dto.Quantity;
            }
            else if (dto.MovementType == "ADJUSTMENT")
                item.QuantityOnHand = dto.Quantity;

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return Ok(ApiResponse<StockMovementResponseDto>.Ok(new StockMovementResponseDto(movement.Id, movement.ProductId, "", movement.WarehouseId, "", movement.MovementType, movement.Quantity, movement.UnitCost, movement.TotalCost, movement.Reference, movement.Notes, movement.MovementDate, movement.CreatedAt)));
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }
}
