using ErpCrm.Application.Common.Models;
using ErpCrm.Application.Common.Interfaces;
using ErpCrm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpCrm.API.Controllers.v1;

[ApiController]
[Route("api/v1/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;

    public NotificationsController(AppDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool? isRead = null)
    {
        var query = _db.Notifications.AsNoTracking()
            .Where(n => n.UserId == _tenantContext.UserId);
        if (isRead.HasValue) query = query.Where(n => n.IsRead == isRead.Value);

        var total = await query.CountAsync();
        var unreadCount = await _db.Notifications
            .CountAsync(n => n.UserId == _tenantContext.UserId && !n.IsRead);

        var items = await query.OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(n => new
            {
                n.Id, n.Title, n.Message, n.Type,
                n.IsRead, n.ReadAt, n.ActionUrl, n.CreatedAt
            }).ToListAsync();

        return Ok(ApiResponse<object>.Ok(new
        {
            Items = items, Total = total,
            UnreadCount = unreadCount, Page = page, PageSize = pageSize
        }));
    }

    [HttpPatch("{id:guid}/read")]
    public async Task<ActionResult<ApiResponse<string>>> MarkRead(Guid id)
    {
        var notification = await _db.Notifications
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == _tenantContext.UserId)
            ?? throw new KeyNotFoundException("Notification not found");
        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<string>.Ok("Marked as read"));
    }

    [HttpPatch("read-all")]
    public async Task<ActionResult<ApiResponse<string>>> MarkAllRead()
    {
        var unread = await _db.Notifications
            .Where(n => n.UserId == _tenantContext.UserId && !n.IsRead)
            .ToListAsync();
        foreach (var n in unread) { n.IsRead = true; n.ReadAt = DateTime.UtcNow; }
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<string>.Ok($"{unread.Count} notifications marked as read"));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(Guid id)
    {
        var notification = await _db.Notifications
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == _tenantContext.UserId)
            ?? throw new KeyNotFoundException("Notification not found");
        _db.Notifications.Remove(notification);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<string>.Ok("Notification deleted"));
    }
}
