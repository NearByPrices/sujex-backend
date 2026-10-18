using ErpCrm.Application.Common.Models;
using ErpCrm.Application.Common.Interfaces;
using ErpCrm.Application.Projects.Dtos;
using ErpCrm.Domain.Entities.Projects;
using ErpCrm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpCrm.API.Controllers.v1.Projects;

[ApiController]
[Route("api/v1/projects")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;

    public ProjectsController(AppDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<ProjectResponseDto>>>> GetAll([FromQuery] QueryParams q, [FromQuery] string? status = null)
    {
        var query = _db.Projects.AsNoTracking().Where(p => !p.IsDeleted);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(p => p.Status == status);
        if (!string.IsNullOrWhiteSpace(q.Search))
            query = query.Where(p => p.Name.Contains(q.Search) || (p.Code != null && p.Code.Contains(q.Search)));

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(p => p.CreatedAt)
            .Skip((q.Page - 1) * q.PageSize).Take(q.PageSize)
            .Select(p => new ProjectResponseDto
            {
                Id = p.Id, CompanyId = p.CompanyId, Name = p.Name, Code = p.Code,
                Description = p.Description, StartDate = p.StartDate, EndDate = p.EndDate,
                Status = p.Status, Budget = p.Budget, ManagerId = p.ManagerId,
                ContactId = p.ContactId, Tags = p.Tags,
                TaskCount = p.Tasks.Count(t => !t.IsDeleted),
                CompletedTaskCount = p.Tasks.Count(t => !t.IsDeleted && t.Status == "Done"),
                CreatedAt = p.CreatedAt
            }).ToListAsync();

        var pagination = new PaginationMeta { Page = q.Page, PageSize = q.PageSize, TotalCount = total };
        return Ok(ApiResponse<PagedResult<ProjectResponseDto>>.OkPaged(
            new PagedResult<ProjectResponseDto> { Items = items, Pagination = pagination }, pagination));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ProjectResponseDto>>> GetById(Guid id)
    {
        var p = await _db.Projects.Include(p => p.Tasks.Where(t => !t.IsDeleted))
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted)
            ?? throw new KeyNotFoundException("Project not found");

        return Ok(ApiResponse<ProjectResponseDto>.Ok(new ProjectResponseDto
        {
            Id = p.Id, CompanyId = p.CompanyId, Name = p.Name, Code = p.Code,
            Description = p.Description, StartDate = p.StartDate, EndDate = p.EndDate,
            Status = p.Status, Budget = p.Budget, ManagerId = p.ManagerId,
            ContactId = p.ContactId, Tags = p.Tags,
            TaskCount = p.Tasks.Count, CompletedTaskCount = p.Tasks.Count(t => t.Status == "Done"),
            CreatedAt = p.CreatedAt
        }));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ProjectResponseDto>>> Create([FromBody] CreateProjectDto dto)
    {
        var project = new Project
        {
            TenantId = _tenantContext.TenantId, CompanyId = dto.CompanyId, Name = dto.Name,
            Code = dto.Code, Description = dto.Description, StartDate = dto.StartDate,
            EndDate = dto.EndDate, Budget = dto.Budget, ManagerId = dto.ManagerId,
            ContactId = dto.ContactId, Tags = dto.Tags
        };
        _db.Projects.Add(project);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = project.Id }, ApiResponse<ProjectResponseDto>.Ok(new ProjectResponseDto
        {
            Id = project.Id, CompanyId = project.CompanyId, Name = project.Name,
            Status = project.Status, CreatedAt = project.CreatedAt
        }));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<string>>> Update(Guid id, [FromBody] UpdateProjectDto dto)
    {
        var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted)
            ?? throw new KeyNotFoundException("Project not found");
        project.Name = dto.Name; project.Code = dto.Code; project.Description = dto.Description;
        project.StartDate = dto.StartDate; project.EndDate = dto.EndDate; project.Status = dto.Status;
        project.Budget = dto.Budget; project.ManagerId = dto.ManagerId; project.ContactId = dto.ContactId;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<string>.Ok("Project updated"));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(Guid id)
    {
        var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted)
            ?? throw new KeyNotFoundException("Project not found");
        project.IsDeleted = true; project.DeletedAt = DateTime.UtcNow; project.DeletedBy = _tenantContext.UserId;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<string>.Ok("Project deleted"));
    }

    [HttpGet("{id:guid}/tasks")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TaskResponseDto>>>> GetTasks(Guid id)
    {
        var tasks = await _db.ProjectTasks.Where(t => t.ProjectId == id && !t.IsDeleted)
            .Select(t => new TaskResponseDto
            {
                Id = t.Id, ProjectId = t.ProjectId, Title = t.Title, Description = t.Description,
                Status = t.Status, Priority = t.Priority, DueDate = t.DueDate,
                CompletedAt = t.CompletedAt, AssignedToUserId = t.AssignedToUserId,
                ParentTaskId = t.ParentTaskId, EstimatedHours = t.EstimatedHours,
                ActualHours = t.ActualHours,
                SubTaskCount = t.SubTasks.Count(s => !s.IsDeleted),
                CreatedAt = t.CreatedAt
            }).ToListAsync();

        return Ok(ApiResponse<IEnumerable<TaskResponseDto>>.Ok(tasks));
    }

    [HttpPost("{id:guid}/tasks")]
    public async Task<ActionResult<ApiResponse<TaskResponseDto>>> CreateTask(Guid id, [FromBody] CreateTaskDto dto)
    {
        var task = new ErpCrm.Domain.Entities.Projects.Task
        {
            TenantId = _tenantContext.TenantId, ProjectId = id, Title = dto.Title,
            Description = dto.Description, Priority = dto.Priority, DueDate = dto.DueDate,
            AssignedToUserId = dto.AssignedToUserId, ParentTaskId = dto.ParentTaskId,
            EstimatedHours = dto.EstimatedHours
        };
        _db.ProjectTasks.Add(task);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<TaskResponseDto>.Ok(new TaskResponseDto
        {
            Id = task.Id, ProjectId = task.ProjectId, Title = task.Title,
            Status = task.Status, Priority = task.Priority, CreatedAt = task.CreatedAt
        }));
    }

    [HttpPut("{projectId:guid}/tasks/{taskId:guid}")]
    public async Task<ActionResult<ApiResponse<string>>> UpdateTask(Guid projectId, Guid taskId, [FromBody] UpdateTaskDto dto)
    {
        var task = await _db.ProjectTasks.FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectId == projectId && !t.IsDeleted)
            ?? throw new KeyNotFoundException("Task not found");
        task.Title = dto.Title; task.Description = dto.Description; task.Status = dto.Status;
        task.Priority = dto.Priority; task.DueDate = dto.DueDate;
        task.AssignedToUserId = dto.AssignedToUserId;
        task.EstimatedHours = dto.EstimatedHours; task.ActualHours = dto.ActualHours;
        if (dto.Status == "Done" && task.CompletedAt == null) task.CompletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<string>.Ok("Task updated"));
    }

    [HttpDelete("{projectId:guid}/tasks/{taskId:guid}")]
    public async Task<ActionResult<ApiResponse<string>>> DeleteTask(Guid projectId, Guid taskId)
    {
        var task = await _db.ProjectTasks.FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectId == projectId && !t.IsDeleted)
            ?? throw new KeyNotFoundException("Task not found");
        task.IsDeleted = true; task.DeletedAt = DateTime.UtcNow; task.DeletedBy = _tenantContext.UserId;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<string>.Ok("Task deleted"));
    }
}
