namespace ErpCrm.Application.Identity.Dtos;

public record CreateRoleDto(string Name, string? Description = null);
public record UpdateRoleDto(string Name, string? Description);
public record AssignPermissionsDto(List<string> Permissions);

public class RoleResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int UserCount { get; set; }
    public List<string> Permissions { get; set; } = new();
}
