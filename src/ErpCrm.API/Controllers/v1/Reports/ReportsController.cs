using ErpCrm.Application.Common.Interfaces;
using ErpCrm.Application.Common.Models;
using ErpCrm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpCrm.API.Controllers.v1.Reports;

[ApiController]
[Route("api/v1/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;

    public ReportsController(AppDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponse<object>>> GetDashboard()
    {
        var data = new
        {
            TotalContacts = await _db.Contacts.CountAsync(c => !c.IsDeleted),
            OpenLeads = await _db.Leads.CountAsync(l => !l.IsDeleted
                && l.Status != ErpCrm.Domain.Enums.LeadStatusEnum.Converted
                && l.Status != ErpCrm.Domain.Enums.LeadStatusEnum.Lost),
            ActiveOpportunities = await _db.Opportunities.CountAsync(o => !o.IsDeleted && o.Status == ErpCrm.Domain.Enums.OpportunityStatus.Open),
            SalesOrdersThisMonth = await _db.SalesOrders.CountAsync(o => !o.IsDeleted
                && o.OrderDate.Month == DateTime.UtcNow.Month
                && o.OrderDate.Year == DateTime.UtcNow.Year),
            TotalProducts = await _db.Products.CountAsync(p => !p.IsDeleted && p.IsActive),
            ActiveEmployees = await _db.Employees.CountAsync(e => !e.IsDeleted && e.Status == ErpCrm.Domain.Enums.EmployeeStatus.Active),
            ActiveProjects = await _db.Projects.CountAsync(p => !p.IsDeleted && p.Status == "Active"),
            PendingLeaveRequests = await _db.LeaveRequests.CountAsync(r => r.Status == ErpCrm.Domain.Enums.ApprovalStatus.Pending)
        };

        return Ok(ApiResponse<object>.Ok(data));
    }

    [HttpGet("crm-pipeline")]
    public async Task<ActionResult<ApiResponse<object>>> GetCrmPipeline()
    {
        var leads = await _db.Leads.Where(l => !l.IsDeleted)
            .GroupBy(l => l.Status)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
            .ToListAsync();

        var opportunities = await _db.Opportunities.Where(o => !o.IsDeleted)
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(new { Leads = leads, Opportunities = opportunities }));
    }

    [HttpGet("sales-summary")]
    public async Task<ActionResult<ApiResponse<object>>> GetSalesSummary([FromQuery] int year = 0)
    {
        if (year == 0) year = DateTime.UtcNow.Year;

        var monthly = await _db.SalesOrders
            .Where(o => !o.IsDeleted && o.OrderDate.Year == year)
            .GroupBy(o => o.OrderDate.Month)
            .Select(g => new { Month = g.Key, Count = g.Count(), Total = g.Sum(o => o.TotalAmount) })
            .OrderBy(g => g.Month)
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(new { Year = year, Monthly = monthly }));
    }
}
