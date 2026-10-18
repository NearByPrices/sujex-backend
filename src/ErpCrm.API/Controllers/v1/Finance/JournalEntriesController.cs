using ErpCrm.Application.Common.Models;
using ErpCrm.Application.Common.Interfaces;
using ErpCrm.Application.Finance.Dtos;
using ErpCrm.Domain.Entities.Finance;
using ErpCrm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpCrm.API.Controllers.v1.Finance;

[ApiController]
[Route("api/v1/finance/journal-entries")]
[Authorize]
public class JournalEntriesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;

    public JournalEntriesController(AppDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<JournalEntryResponseDto>>>> GetAll([FromQuery] QueryParams q)
    {
        var query = _db.JournalEntries.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(q.Search))
            query = query.Where(j => j.EntryNumber.Contains(q.Search) || (j.Description != null && j.Description.Contains(q.Search)));

        var total = await query.CountAsync();
        var items = await query.Include(j => j.Lines).ThenInclude(l => l.Account)
            .OrderByDescending(j => j.EntryDate)
            .Skip((q.Page - 1) * q.PageSize).Take(q.PageSize)
            .Select(j => new JournalEntryResponseDto
            {
                Id = j.Id, EntryNumber = j.EntryNumber, EntryDate = j.EntryDate,
                Description = j.Description, Reference = j.Reference, ReferenceType = j.ReferenceType,
                IsPosted = j.IsPosted, PostedAt = j.PostedAt,
                TotalDebit = j.TotalDebit, TotalCredit = j.TotalCredit, CreatedAt = j.CreatedAt
            }).ToListAsync();

        var pagination = new PaginationMeta { Page = q.Page, PageSize = q.PageSize, TotalCount = total };
        return Ok(ApiResponse<PagedResult<JournalEntryResponseDto>>.OkPaged(
            new PagedResult<JournalEntryResponseDto> { Items = items, Pagination = pagination }, pagination));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<JournalEntryResponseDto>>> GetById(Guid id)
    {
        var j = await _db.JournalEntries.Include(j => j.Lines).ThenInclude(l => l.Account)
            .FirstOrDefaultAsync(j => j.Id == id)
            ?? throw new KeyNotFoundException("Journal entry not found");

        return Ok(ApiResponse<JournalEntryResponseDto>.Ok(new JournalEntryResponseDto
        {
            Id = j.Id, EntryNumber = j.EntryNumber, EntryDate = j.EntryDate,
            Description = j.Description, Reference = j.Reference, ReferenceType = j.ReferenceType,
            IsPosted = j.IsPosted, PostedAt = j.PostedAt,
            TotalDebit = j.TotalDebit, TotalCredit = j.TotalCredit, CreatedAt = j.CreatedAt,
            Lines = j.Lines.Select(l => new JournalLineResponseDto
            {
                Id = l.Id, AccountId = l.AccountId, AccountCode = l.Account.Code,
                AccountName = l.Account.Name, Description = l.Description,
                Debit = l.Debit, Credit = l.Credit
            }).ToList()
        }));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<JournalEntryResponseDto>>> Create([FromBody] CreateJournalEntryDto dto)
    {
        if (dto.Lines?.Any() == true)
        {
            var totalDebit = dto.Lines.Sum(l => l.Debit);
            var totalCredit = dto.Lines.Sum(l => l.Credit);
            if (totalDebit != totalCredit)
                throw new InvalidOperationException("Journal entry must be balanced: total debits must equal total credits");
        }

        var entry = new JournalEntry
        {
            TenantId = _tenantContext.TenantId,
            EntryNumber = $"JE-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}",
            EntryDate = dto.EntryDate, Description = dto.Description,
            Reference = dto.Reference, ReferenceType = dto.ReferenceType
        };

        if (dto.Lines?.Any() == true)
        {
            int sort = 1;
            decimal totalDebit = 0, totalCredit = 0;
            foreach (var lineDto in dto.Lines)
            {
                totalDebit += lineDto.Debit; totalCredit += lineDto.Credit;
                entry.Lines.Add(new JournalLine
                {
                    TenantId = _tenantContext.TenantId,
                    AccountId = lineDto.AccountId, Description = lineDto.Description,
                    Debit = lineDto.Debit, Credit = lineDto.Credit, SortOrder = sort++
                });
            }
            entry.TotalDebit = totalDebit; entry.TotalCredit = totalCredit;
        }

        _db.JournalEntries.Add(entry);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = entry.Id }, ApiResponse<JournalEntryResponseDto>.Ok(new JournalEntryResponseDto
        {
            Id = entry.Id, EntryNumber = entry.EntryNumber, EntryDate = entry.EntryDate,
            Description = entry.Description, IsPosted = entry.IsPosted,
            TotalDebit = entry.TotalDebit, TotalCredit = entry.TotalCredit, CreatedAt = entry.CreatedAt
        }));
    }

    [HttpPatch("{id:guid}/post")]
    public async Task<ActionResult<ApiResponse<string>>> Post(Guid id)
    {
        var entry = await _db.JournalEntries.FindAsync(id) ?? throw new KeyNotFoundException("Journal entry not found");
        if (entry.IsPosted) throw new InvalidOperationException("Entry is already posted");
        entry.IsPosted = true; entry.PostedAt = DateTime.UtcNow; entry.PostedByUserId = _tenantContext.UserId;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<string>.Ok("Journal entry posted"));
    }
}
