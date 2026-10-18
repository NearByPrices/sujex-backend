using ErpCrm.Application.Common.Models;
using ErpCrm.Application.Common.Interfaces;
using ErpCrm.Application.Purchase.Dtos;
using ErpCrm.Domain.Entities.Purchase;
using ErpCrm.Domain.Enums;
using ErpCrm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpCrm.API.Controllers.v1.Purchase;

[ApiController]
[Route("api/v1/purchase/orders")]
[Authorize]
public class PurchaseOrdersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;

    public PurchaseOrdersController(AppDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<PurchaseOrderResponseDto>>>> GetAll([FromQuery] QueryParams q, [FromQuery] PurchaseOrderStatus? status = null)
    {
        var query = _db.PurchaseOrders.AsNoTracking().Where(o => !o.IsDeleted);
        if (status.HasValue) query = query.Where(o => o.Status == status);
        if (!string.IsNullOrWhiteSpace(q.Search))
            query = query.Where(o => o.OrderNumber.Contains(q.Search));

        var total = await query.CountAsync();
        var items = await query.Include(o => o.Vendor)
            .OrderByDescending(o => o.OrderDate)
            .Skip((q.Page - 1) * q.PageSize).Take(q.PageSize)
            .Select(o => new PurchaseOrderResponseDto
            {
                Id = o.Id, OrderNumber = o.OrderNumber, OrderDate = o.OrderDate,
                VendorId = o.VendorId, VendorName = o.Vendor.Name, Status = o.Status,
                ExpectedDeliveryDate = o.ExpectedDeliveryDate, SubTotal = o.SubTotal,
                TaxAmount = o.TaxAmount, TotalAmount = o.TotalAmount, CurrencyCode = o.CurrencyCode,
                Notes = o.Notes, CreatedAt = o.CreatedAt
            }).ToListAsync();

        var pagination = new PaginationMeta { Page = q.Page, PageSize = q.PageSize, TotalCount = total };
        return Ok(ApiResponse<PagedResult<PurchaseOrderResponseDto>>.OkPaged(
            new PagedResult<PurchaseOrderResponseDto> { Items = items, Pagination = pagination }, pagination));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<PurchaseOrderResponseDto>>> GetById(Guid id)
    {
        var o = await _db.PurchaseOrders.Include(o => o.Vendor).Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted)
            ?? throw new KeyNotFoundException("Purchase order not found");

        return Ok(ApiResponse<PurchaseOrderResponseDto>.Ok(new PurchaseOrderResponseDto
        {
            Id = o.Id, OrderNumber = o.OrderNumber, OrderDate = o.OrderDate,
            VendorId = o.VendorId, VendorName = o.Vendor.Name, Status = o.Status,
            ExpectedDeliveryDate = o.ExpectedDeliveryDate, SubTotal = o.SubTotal,
            TaxAmount = o.TaxAmount, TotalAmount = o.TotalAmount, CurrencyCode = o.CurrencyCode,
            Notes = o.Notes, CreatedAt = o.CreatedAt,
            Lines = o.Lines.Select(l => new PurchaseOrderLineResponseDto
            {
                Id = l.Id, ProductId = l.ProductId, Description = l.Description,
                Quantity = l.Quantity, ReceivedQuantity = l.ReceivedQuantity,
                UnitCost = l.UnitCost, TaxPercent = l.TaxPercent, LineTotal = l.LineTotal
            }).ToList()
        }));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PurchaseOrderResponseDto>>> Create([FromBody] CreatePurchaseOrderDto dto)
    {
        var order = new PurchaseOrder
        {
            TenantId = _tenantContext.TenantId, VendorId = dto.VendorId,
            OrderNumber = $"PO-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}",
            OrderDate = dto.OrderDate ?? DateTime.UtcNow,
            ExpectedDeliveryDate = dto.ExpectedDeliveryDate, CurrencyCode = dto.CurrencyCode, Notes = dto.Notes
        };

        if (dto.Lines?.Any() == true)
        {
            decimal subTotal = 0, taxTotal = 0;
            int sort = 1;
            foreach (var lineDto in dto.Lines)
            {
                subTotal += lineDto.Quantity * lineDto.UnitCost;
                taxTotal += lineDto.Quantity * lineDto.UnitCost * (lineDto.TaxPercent / 100m);
                order.Lines.Add(new PurchaseOrderLine
                {
                    TenantId = _tenantContext.TenantId,
                    ProductId = lineDto.ProductId, Description = lineDto.Description,
                    Quantity = lineDto.Quantity, UnitCost = lineDto.UnitCost,
                    TaxPercent = lineDto.TaxPercent, SortOrder = sort++
                });
            }
            order.SubTotal = subTotal; order.TaxAmount = taxTotal; order.TotalAmount = subTotal + taxTotal;
        }

        _db.PurchaseOrders.Add(order);
        await _db.SaveChangesAsync();

        var vendor = await _db.Vendors.FindAsync(dto.VendorId);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, ApiResponse<PurchaseOrderResponseDto>.Ok(new PurchaseOrderResponseDto
        {
            Id = order.Id, OrderNumber = order.OrderNumber, OrderDate = order.OrderDate,
            VendorId = order.VendorId, VendorName = vendor?.Name ?? string.Empty,
            Status = order.Status, TotalAmount = order.TotalAmount, CurrencyCode = order.CurrencyCode, CreatedAt = order.CreatedAt
        }));
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<ApiResponse<string>>> UpdateStatus(Guid id, [FromBody] UpdatePurchaseOrderStatusDto dto)
    {
        var order = await _db.PurchaseOrders.FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted)
            ?? throw new KeyNotFoundException("Purchase order not found");
        order.Status = dto.Status;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<string>.Ok("Status updated"));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(Guid id)
    {
        var order = await _db.PurchaseOrders.FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted)
            ?? throw new KeyNotFoundException("Purchase order not found");
        if (order.Status != PurchaseOrderStatus.Draft)
            throw new InvalidOperationException("Only draft orders can be deleted");
        order.IsDeleted = true; order.DeletedAt = DateTime.UtcNow; order.DeletedBy = _tenantContext.UserId;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<string>.Ok("Purchase order deleted"));
    }
}
