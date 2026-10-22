using ErpCrm.Application.Auth;
using ErpCrm.Application.Auth.Dtos;
using ErpCrm.Application.Common.Interfaces;
using ErpCrm.Domain.Entities.Identity;
using ErpCrm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ErpCrm.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly AppDbContext _db;
    private readonly IJwtTokenService _jwtService;
    private readonly IModuleService _moduleService;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthService> _logger;
    private readonly IEmailSender _emailSender;

    public AuthService(
        UserManager<AppUser> userManager,
        AppDbContext db,
        IJwtTokenService jwtService,
        IModuleService moduleService,
        IConfiguration config,
        ILogger<AuthService> logger,
        IEmailSender emailSender)
    {
        _userManager = userManager;
        _db = db;
        _jwtService = jwtService;
        _moduleService = moduleService;
        _config = config;
        _logger = logger;
        _emailSender = emailSender;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email)
            ?? throw new UnauthorizedAccessException("Invalid credentials");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account is inactive");

        if (!await _userManager.CheckPasswordAsync(user, dto.Password))
            throw new UnauthorizedAccessException("Invalid credentials");

        if (user.IsPlatformAdmin)
            throw new UnauthorizedAccessException("Use platform login endpoint");

        return await GenerateAuthResponseAsync(user, dto.CompanyId);
    }

    public async Task<AuthResponseDto> PlatformLoginAsync(PlatformLoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email)
            ?? throw new UnauthorizedAccessException("Invalid credentials");

        if (!user.IsPlatformAdmin)
            throw new UnauthorizedAccessException("Not a platform admin");

        if (!await _userManager.CheckPasswordAsync(user, dto.Password))
            throw new UnauthorizedAccessException("Invalid credentials");

        return await GenerateAuthResponseAsync(user, null);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
    {
        var user = await _db.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken)
            ?? throw new UnauthorizedAccessException("Invalid refresh token");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account is inactive");

        if (user.RefreshTokenExpiry < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token expired");

        return await GenerateAuthResponseAsync(user, null);
    }

    public async Task ForgotPasswordAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return;

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var baseUrl = _config["PasswordReset:BaseUrl"]?.TrimEnd('/')
            ?? throw new InvalidOperationException("Password reset base URL is not configured");
        var resetUrl = $"{baseUrl}?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";
        await _emailSender.SendAsync(email, "Reset your Sujex password",
            $"<p>Reset your password by clicking <a href=\"{resetUrl}\">this link</a>.</p>");
    }

    public async Task ResetPasswordAsync(ResetPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email)
            ?? throw new KeyNotFoundException("User not found");

        var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new KeyNotFoundException("User not found");

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    public async Task LogoutAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user != null)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            await _userManager.UpdateAsync(user);
        }
    }

    public async Task<UserInfoDto> GetCurrentUserAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new KeyNotFoundException("User not found");

        return await BuildUserInfoDto(user, null);
    }

    private async Task<AuthResponseDto> GenerateAuthResponseAsync(AppUser user, Guid? companyId)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var permissions = await GetUserPermissionsAsync(user.Id);

        var accessToken = _jwtService.GenerateAccessToken(user, roles, permissions);
        var refreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(
            int.Parse(_config["Jwt:RefreshTokenExpiryDays"] ?? "30"));
        await _userManager.UpdateAsync(user);

        var expiry = DateTime.UtcNow.AddMinutes(
            int.Parse(_config["Jwt:AccessTokenExpiryMinutes"] ?? "60"));

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiry = expiry,
            User = await BuildUserInfoDto(user, companyId, roles, permissions)
        };
    }

    private async Task<UserInfoDto> BuildUserInfoDto(
        AppUser user,
        Guid? companyId,
        IList<string>? roles = null,
        IList<string>? permissions = null)
    {
        roles ??= await _userManager.GetRolesAsync(user);
        permissions ??= await GetUserPermissionsAsync(user.Id);

        IEnumerable<string> enabledModules = Enumerable.Empty<string>();
        if (user.TenantId.HasValue)
        {
            var modules = await _moduleService.GetEnabledModulesAsync(user.TenantId.Value);
            enabledModules = modules.Select(m => m.ToString());
        }

        return new UserInfoDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            AvatarUrl = user.AvatarUrl,
            IsPlatformAdmin = user.IsPlatformAdmin,
            TenantId = user.TenantId,
            CompanyId = companyId,
            Roles = roles,
            Permissions = permissions,
            EnabledModules = enabledModules
        };
    }

    private async Task<IList<string>> GetUserPermissionsAsync(Guid userId)
    {
        var userRoleIds = await _db.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync();

        var permissions = await _db.RolePermissions
            .Where(rp => userRoleIds.Contains(rp.RoleId))
            .Select(rp => rp.Permission.PermissionCode)
            .Distinct()
            .ToListAsync();

        return permissions;
    }
}
