namespace ErpCrm.Application.Identity.Dtos;

public record InviteUserDto(string Email, string FirstName, string LastName, string Password, Guid? CompanyId = null, string Role = "User");
public record UpdateUserDto(string FirstName, string LastName, string? Phone = null, bool IsActive = true);
public record ChangeUserRoleDto(string Role);

public class UserResponseDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public bool IsActive { get; set; }
    public bool EmailConfirmed { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
    public DateTime CreatedAt { get; set; }
}
