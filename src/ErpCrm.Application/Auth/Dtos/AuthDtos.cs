namespace ErpCrm.Application.Auth.Dtos;

public record LoginDto(string Email, string Password, Guid? CompanyId = null);
public record PlatformLoginDto(string Email, string Password);
public record RefreshRequestDto(string RefreshToken);
public record ForgotPasswordDto(string Email);
public record ResetPasswordDto(string Email, string Token, string NewPassword);
public record ChangePasswordDto(string CurrentPassword, string NewPassword);

public class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiry { get; set; }
    public UserInfoDto User { get; set; } = null!;
}

public class UserInfoDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public bool IsPlatformAdmin { get; set; }
    public Guid? TenantId { get; set; }
    public Guid? CompanyId { get; set; }
    public IEnumerable<string> Roles { get; set; } = Enumerable.Empty<string>();
    public IEnumerable<string> Permissions { get; set; } = Enumerable.Empty<string>();
    public IEnumerable<string> EnabledModules { get; set; } = Enumerable.Empty<string>();
}
