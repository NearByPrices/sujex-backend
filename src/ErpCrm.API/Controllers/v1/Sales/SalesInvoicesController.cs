using ErpCrm.Application.Common.Models;
using ErpCrm.Application.Common.Interfaces;
using ErpCrm.Application.Sales.Dtos;
using ErpCrm.Domain.Entities.Sales;
using ErpCrm.Domain.Enums;
using ErpCrm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpCrm.API.Controllers.v1.Sales;

[ApiController]
[Route("api/v1/sales/invoices")]
[Authorize]
public class SalesInvoicesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;

    public SalesInvoicesController(AppDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<SalesInvoiceResponseDto>>>> GetAll(
        [FromQuery] QueryParams q,
        [FromQuery] InvoiceStatus? status = null)
    {
        var query = _db.SalesInvoices.AsNoTracking().Where(i => !i.IsDeleted);
        if (status.HasValue) query = query.Where(i => i.Status == status);
        if (!string.IsNullOrWhiteSpace(q.Search))
            query = query.Where(i => i.InvoiceNumber.Contains(q.Search));

        var total = await query.CountAsync();
        var items = await query.Include(i => i.SalesOrder)
            .OrderByDescending(i => i.InvoiceDate)
            .Skip((q.Page - 1) * q.PageSize).Take(q.PageSize)
            .Select(i => new SalesInvoiceResponseDto
            {
                Id = i.Id, CompanyId = i.CompanyId, InvoiceNumber = i.InvoiceNumber,
                SalesOrderId = i.SalesOrderId,
                SalesOrderNumber = i.SalesOrder != null ? i.SalesOrder.OrderNumber : null,
                ContactId = i.ContactId,
                InvoiceDate = i.InvoiceDate, DueDate = i.DueDate, Status = i.Status,
                SubTotal = i.SubTotal, TaxAmount = i.TaxAmount, TotalAmount = i.TotalAmount,
                PaidAmount = i.PaidAmount, BalanceDue = i.BalanceDue,
                CurrencyCode = i.CurrencyCode, Notes = i.Notes, CreatedAt = i.CreatedAt
            }).ToListAsync();

        var pagination = new PaginationMeta { Page = q.Page, PageSize = q.PageSize, TotalCount = total };
        return Ok(ApiResponse<PagedResult<SalesInvoiceResponseDto>>.OkPaged(
            new PagedResult<SalesInvoiceResponseDto> { Items = items, Pagination = pagination }, pagination));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<SalesInvoiceResponseDto>>> GetById(Guid id)
    {
        var i = await _db.SalesInvoices.Include(i => i.SalesOrder).Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted)
            ?? throw new KeyNotFoundException("Invoice not found");

        return Ok(ApiResponse<SalesInvoiceResponseDto>.Ok(new SalesInvoiceResponseDto
        {
            Id = i.Id, CompanyId = i.CompanyId, InvoiceNumber = i.InvoiceNumber,
            SalesOrderId = i.SalesOrderId, SalesOrderNumber = i.SalesOrder?.OrderNumber,
            ContactId = i.ContactId, InvoiceDate = i.InvoiceDate, DueDate = i.DueDate,
            Status = i.Status, SubTotal = i.SubTotal, TaxAmount = i.TaxAmount,
            TotalAmount = i.TotalAmount, PaidAmount = i.PaidAmount, BalanceDue = i.BalanceDue,
            CurrencyCode = i.CurrencyCode, Notes = i.Notes, CreatedAt = i.CreatedAt
        }));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<SalesInvoiceResponseDto>>> Create([FromBody] CreateSalesInvoiceDto dto)
    {
        decimal subTotal = 0, taxAmount = 0, totalAmount = 0;

        if (dto.SalesOrderId.HasValue)
        {
            var order = await _db.SalesOrders.FirstOrDefaultAsync(o => o.Id == dto.SalesOrderId);
            if (order != null)
            {
                subTotal = order.SubTotal;
                taxAmount = order.TaxAmount;
                totalAmount = order.TotalAmount;
            }
        }

        var invoice = new SalesInvoice
        {
            TenantId = _tenantContext.TenantId, CompanyId = dto.CompanyId,
            InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}",
            SalesOrderId = dto.SalesOrderId, ContactId = dto.ContactId,
            InvoiceDate = dto.InvoiceDate ?? DateTime.UtcNow,
            DueDate = dto.DueDate ?? DateTime.UtcNow.AddDays(30),
            CurrencyCode = dto.CurrencyCode, Notes = dto.Notes,
            SubTotal = subTotal, TaxAmount = taxAmount, TotalAmount = totalAmount
        };
        _db.SalesInvoices.Add(invoice);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = invoice.Id }, ApiResponse<SalesInvoiceResponseDto>.Ok(new SalesInvoiceResponseDto
        {
            Id = invoice.Id, CompanyId = invoice.CompanyId, InvoiceNumber = invoice.InvoiceNumber,
            InvoiceDate = invoice.InvoiceDate, DueDate = invoice.DueDate, Status = invoice.Status,
            TotalAmount = invoice.TotalAmount, CurrencyCode = invoice.CurrencyCode, CreatedAt = invoice.CreatedAt
        }));
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<ApiResponse<string>>> UpdateStatus(Guid id, [FromBody] UpdateSalesInvoiceStatusDto dto)
    {
        var invoice = await _db.SalesInvoices.FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted)
            ?? throw new KeyNotFoundException("Invoice not found");
        invoice.Status = dto.Status;
        if (dto.Status == InvoiceStatus.Paid) invoice.PaidAmount = invoice.TotalAmount;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<string>.Ok("Invoice status updated"));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(Guid id)
    {
        var invoice = await _db.SalesInvoices.FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted)
            ?? throw new KeyNotFoundException("Invoice not found");
        if (invoice.Status != InvoiceStatus.Draft)
            return BadRequest(ApiResponse<string>.Fail("Only draft invoices can be deleted"));
        invoice.IsDeleted = true;
        invoice.DeletedAt = DateTime.UtcNow;
        invoice.DeletedBy = _tenantContext.UserId;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<string>.Ok("Invoice deleted"));
    }
}
