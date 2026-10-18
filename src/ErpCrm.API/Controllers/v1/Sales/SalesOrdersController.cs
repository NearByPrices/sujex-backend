using ErpCrm.Application.Common.Interfaces;
using ErpCrm.Application.Common.Models;
using ErpCrm.Application.Sales.Dtos;
using ErpCrm.Domain.Entities.Sales;
using ErpCrm.Domain.Enums;
using ErpCrm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpCrm.API.Controllers.v1.Sales;

[Authorize]
[ApiController]
[Route("api/v1/sales-orders")]
public class SalesOrdersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenant;
    private readonly Application.Common.Interfaces.INumberSequenceService _seq;

    public SalesOrdersController(AppDbContext db, ITenantContext tenant, Application.Common.Interfaces.INumberSequenceService seq)
    {
        _db = db;
        _tenant = tenant;
        _seq = seq;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null, [FromQuery] SalesOrderStatus? status = null)
    {
        var query = _db.SalesOrders.Where(s => !s.IsDeleted);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(s => s.OrderNumber.Contains(search));
        if (status.HasValue)
            query = query.Where(s => s.Status == status.Value);

        var total = await query.CountAsync();
        var items = await query.Include(s => s.Lines).OrderByDescending(s => s.OrderDate)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(s => new SalesOrderResponseDto(s.Id, s.OrderNumber, s.ContactId, s.CompanyId, s.Status, s.OrderDate, s.DeliveryDate, s.SubTotal, s.DiscountAmount, s.TaxAmount, s.TotalAmount, s.Notes,
                s.Lines.Select(l => new SalesOrderLineResponseDto(l.Id, l.ProductId, l.Description, l.Quantity, l.UnitPrice, l.DiscountPercent, l.TaxPercent, l.LineTotal, l.SortOrder)).ToList(),
                s.CreatedAt))
            .ToListAsync();

        return Ok(ApiResponse<List<SalesOrderResponseDto>>.OkPaged(items, new PaginationMeta { Page = page, PageSize = pageSize, TotalCount = total }));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var s = await _db.SalesOrders.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (s == null) return NotFound(ApiResponse<object>.Fail("Sales order not found"));
        return Ok(ApiResponse<SalesOrderResponseDto>.Ok(new SalesOrderResponseDto(s.Id, s.OrderNumber, s.ContactId, s.CompanyId, s.Status, s.OrderDate, s.DeliveryDate, s.SubTotal, s.DiscountAmount, s.TaxAmount, s.TotalAmount, s.Notes,
            s.Lines.Select(l => new SalesOrderLineResponseDto(l.Id, l.ProductId, l.Description, l.Quantity, l.UnitPrice, l.DiscountPercent, l.TaxPercent, l.LineTotal, l.SortOrder)).ToList(),
            s.CreatedAt)));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSalesOrderDto dto)
    {
        var orderNumber = await _seq.GetNextAsync(_tenant.TenantId, dto.CompanyId ?? Guid.Empty, "SO");
        var order = new SalesOrder
        {
            TenantId = _tenant.TenantId,
            OrderNumber = orderNumber,
            ContactId = dto.ContactId,
            CompanyId = dto.CompanyId,
            OrderDate = dto.OrderDate,
            DeliveryDate = dto.DeliveryDate,
            BillingAddress = dto.BillingAddress,
            ShippingAddress = dto.ShippingAddress,
            CurrencyCode = dto.CurrencyCode,
            ExchangeRate = dto.ExchangeRate,
            Notes = dto.Notes,
            AssignedToUserId = dto.AssignedToUserId
        };

        foreach (var lineDto in dto.Lines)
        {
            order.Lines.Add(new SalesOrderLine
            {
                TenantId = _tenant.TenantId,
                ProductId = lineDto.ProductId,
                Description = lineDto.Description,
                Quantity = lineDto.Quantity,
                UnitPrice = lineDto.UnitPrice,
                DiscountPercent = lineDto.DiscountPercent,
                TaxPercent = lineDto.TaxPercent,
                SortOrder = lineDto.SortOrder
            });
        }

        order.SubTotal = order.Lines.Sum(l => l.Quantity * l.UnitPrice);
        order.DiscountAmount = order.Lines.Sum(l => l.Quantity * l.UnitPrice * l.DiscountPercent / 100);
        order.TaxAmount = order.Lines.Sum(l => l.Quantity * l.UnitPrice * (1 - l.DiscountPercent / 100) * l.TaxPercent / 100);
        order.TotalAmount = order.Lines.Sum(l => l.LineTotal);

        _db.SalesOrders.Add(order);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = order.Id },
            ApiResponse<SalesOrderResponseDto>.Ok(new SalesOrderResponseDto(order.Id, order.OrderNumber, order.ContactId, order.CompanyId, order.Status, order.OrderDate, order.DeliveryDate, order.SubTotal, order.DiscountAmount, order.TaxAmount, order.TotalAmount, order.Notes,
                order.Lines.Select(l => new SalesOrderLineResponseDto(l.Id, l.ProductId, l.Description, l.Quantity, l.UnitPrice, l.DiscountPercent, l.TaxPercent, l.LineTotal, l.SortOrder)).ToList(),
                order.CreatedAt)));
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] SalesOrderStatus status)
    {
        var order = await _db.SalesOrders.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (order == null) return NotFound(ApiResponse<object>.Fail("Sales order not found"));
        order.Status = status;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(null!, "Status updated"));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var order = await _db.SalesOrders.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (order == null) return NotFound(ApiResponse<object>.Fail("Sales order not found"));
        if (order.Status != SalesOrderStatus.Draft)
            return BadRequest(ApiResponse<object>.Fail("Only draft orders can be deleted"));
        order.IsDeleted = true;
        order.DeletedAt = DateTime.UtcNow;
        order.DeletedBy = _tenant.UserId;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(null!, "Sales order deleted"));
    }
}
