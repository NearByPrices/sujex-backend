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
[Route("api/v1/finance/accounts")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;

    public AccountsController(AppDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<AccountResponseDto>>>> GetAll([FromQuery] string? accountType = null)
    {
        var query = _db.Accounts.AsNoTracking().Where(a => a.IsActive);
        if (!string.IsNullOrWhiteSpace(accountType)) query = query.Where(a => a.AccountType == accountType);

        var accounts = await query.Include(a => a.Parent)
            .OrderBy(a => a.Code)
            .Select(a => new AccountResponseDto
            {
                Id = a.Id, Code = a.Code, Name = a.Name, AccountType = a.AccountType,
                ParentId = a.ParentId, ParentName = a.Parent != null ? a.Parent.Name : null,
                Balance = a.Balance, IsActive = a.IsActive, IsSystemAccount = a.IsSystemAccount,
                Description = a.Description
            }).ToListAsync();

        return Ok(ApiResponse<IEnumerable<AccountResponseDto>>.Ok(accounts));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<AccountResponseDto>>> GetById(Guid id)
    {
        var a = await _db.Accounts.Include(a => a.Parent)
            .FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new KeyNotFoundException("Account not found");

        return Ok(ApiResponse<AccountResponseDto>.Ok(new AccountResponseDto
        {
            Id = a.Id, Code = a.Code, Name = a.Name, AccountType = a.AccountType,
            ParentId = a.ParentId, ParentName = a.Parent?.Name,
            Balance = a.Balance, IsActive = a.IsActive, IsSystemAccount = a.IsSystemAccount,
            Description = a.Description
        }));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<AccountResponseDto>>> Create([FromBody] CreateAccountDto dto)
    {
        if (await _db.Accounts.AnyAsync(a => a.TenantId == _tenantContext.TenantId && a.Code == dto.Code))
            return BadRequest(ApiResponse<AccountResponseDto>.Fail("Account code already exists"));

        var account = new Account
        {
            TenantId = _tenantContext.TenantId, Code = dto.Code, Name = dto.Name,
            AccountType = dto.AccountType, ParentId = dto.ParentId, Description = dto.Description
        };
        _db.Accounts.Add(account);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = account.Id }, ApiResponse<AccountResponseDto>.Ok(new AccountResponseDto
        {
            Id = account.Id, Code = account.Code, Name = account.Name,
            AccountType = account.AccountType, IsActive = account.IsActive, Balance = account.Balance
        }));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<string>>> Update(Guid id, [FromBody] UpdateAccountDto dto)
    {
        var account = await _db.Accounts.FindAsync(id) ?? throw new KeyNotFoundException("Account not found");
        if (account.IsSystemAccount) throw new InvalidOperationException("System accounts cannot be modified");
        account.Code = dto.Code; account.Name = dto.Name; account.AccountType = dto.AccountType;
        account.ParentId = dto.ParentId; account.IsActive = dto.IsActive; account.Description = dto.Description;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<string>.Ok("Account updated"));
    }
}
