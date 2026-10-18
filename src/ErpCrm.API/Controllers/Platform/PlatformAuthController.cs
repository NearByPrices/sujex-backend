using ErpCrm.Application.Auth;
using ErpCrm.Application.Auth.Dtos;
using ErpCrm.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ErpCrm.API.Controllers.Platform;

[ApiController]
[Route("api/platform/auth")]
[EnableRateLimiting("auth")]
public class PlatformAuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public PlatformAuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] PlatformLoginDto dto)
    {
        var result = await _authService.PlatformLoginAsync(dto);
        return Ok(ApiResponse<AuthResponseDto>.Ok(result));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Refresh([FromBody] RefreshRequestDto dto)
    {
        var result = await _authService.RefreshTokenAsync(dto.RefreshToken);
        return Ok(ApiResponse<AuthResponseDto>.Ok(result));
    }
}
