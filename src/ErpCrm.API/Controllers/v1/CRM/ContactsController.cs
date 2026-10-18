using ErpCrm.Application.Common.Interfaces;
using ErpCrm.Application.Common.Models;
using ErpCrm.Application.CRM.Dtos;
using ErpCrm.Domain.Entities.CRM;
using ErpCrm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpCrm.API.Controllers.v1.CRM;

[Authorize]
[ApiController]
[Route("api/v1/contacts")]
public class ContactsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenant;

    public ContactsController(AppDbContext db, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
    {
        var query = _db.Contacts.Where(c => !c.IsDeleted);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.FirstName.Contains(search) || c.LastName.Contains(search) || c.Email!.Contains(search));

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(c => new ContactResponseDto(c.Id, c.FirstName, c.LastName, c.FullName, c.Email, c.Phone, c.Mobile, c.JobTitle, c.Department, c.AccountName, c.AvatarUrl, c.Notes, c.Tags, c.IsActive, c.CompanyId, c.CreatedAt))
            .ToListAsync();

        return Ok(ApiResponse<List<ContactResponseDto>>.OkPaged(items, new PaginationMeta { Page = page, PageSize = pageSize, Total = total }));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var c = await _db.Contacts.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (c == null) return NotFound(ApiResponse<object>.Fail("Contact not found"));
        return Ok(ApiResponse<ContactResponseDto>.Ok(new ContactResponseDto(c.Id, c.FirstName, c.LastName, c.FullName, c.Email, c.Phone, c.Mobile, c.JobTitle, c.Department, c.AccountName, c.AvatarUrl, c.Notes, c.Tags, c.IsActive, c.CompanyId, c.CreatedAt)));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateContactDto dto)
    {
        var contact = new Contact
        {
            TenantId = _tenant.TenantId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            Mobile = dto.Mobile,
            JobTitle = dto.JobTitle,
            Department = dto.Department,
            AccountName = dto.AccountName,
            Notes = dto.Notes,
            Tags = dto.Tags,
            CompanyId = dto.CompanyId
        };
        _db.Contacts.Add(contact);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = contact.Id },
            ApiResponse<ContactResponseDto>.Ok(new ContactResponseDto(contact.Id, contact.FirstName, contact.LastName, contact.FullName, contact.Email, contact.Phone, contact.Mobile, contact.JobTitle, contact.Department, contact.AccountName, contact.AvatarUrl, contact.Notes, contact.Tags, contact.IsActive, contact.CompanyId, contact.CreatedAt)));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateContactDto dto)
    {
        var contact = await _db.Contacts.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (contact == null) return NotFound(ApiResponse<object>.Fail("Contact not found"));

        contact.FirstName = dto.FirstName;
        contact.LastName = dto.LastName;
        contact.Email = dto.Email;
        contact.Phone = dto.Phone;
        contact.Mobile = dto.Mobile;
        contact.JobTitle = dto.JobTitle;
        contact.Department = dto.Department;
        contact.AccountName = dto.AccountName;
        contact.Notes = dto.Notes;
        contact.Tags = dto.Tags;
        contact.IsActive = dto.IsActive;

        await _db.SaveChangesAsync();
        return Ok(ApiResponse<ContactResponseDto>.Ok(new ContactResponseDto(contact.Id, contact.FirstName, contact.LastName, contact.FullName, contact.Email, contact.Phone, contact.Mobile, contact.JobTitle, contact.Department, contact.AccountName, contact.AvatarUrl, contact.Notes, contact.Tags, contact.IsActive, contact.CompanyId, contact.CreatedAt)));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var contact = await _db.Contacts.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (contact == null) return NotFound(ApiResponse<object>.Fail("Contact not found"));
        contact.IsDeleted = true;
        contact.DeletedAt = DateTime.UtcNow;
        contact.DeletedBy = _tenant.UserId;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(null!, "Contact deleted"));
    }
}
