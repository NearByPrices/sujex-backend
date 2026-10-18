using ErpCrm.Application.Auth.Dtos;

namespace ErpCrm.Application.Auth;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<AuthResponseDto> PlatformLoginAsync(PlatformLoginDto dto);
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);
    Task ForgotPasswordAsync(string email);
    Task ResetPasswordAsync(ResetPasswordDto dto);
    Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto);
    Task LogoutAsync(Guid userId);
    Task<UserInfoDto> GetCurrentUserAsync(Guid userId);
}
