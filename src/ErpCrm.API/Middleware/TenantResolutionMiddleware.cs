using ErpCrm.Infrastructure.Services;
using System.Security.Claims;

namespace ErpCrm.API.Middleware;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, TenantContext tenantContext)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var claims = context.User.Claims;

            var userIdClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub");
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                tenantContext.UserId = userId;

            var tenantIdClaim = claims.FirstOrDefault(c => c.Type == "tenant_id");
            if (tenantIdClaim != null && Guid.TryParse(tenantIdClaim.Value, out var tenantId))
                tenantContext.TenantId = tenantId;

            var companyIdClaim = claims.FirstOrDefault(c => c.Type == "company_id");
            if (companyIdClaim != null && Guid.TryParse(companyIdClaim.Value, out var companyId))
                tenantContext.CompanyId = companyId;

            var platformAdminClaim = claims.FirstOrDefault(c => c.Type == "platform_admin");
            tenantContext.IsPlatformAdmin = platformAdminClaim?.Value == "true";
        }

        await _next(context);
    }
}
