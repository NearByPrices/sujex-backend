using ErpCrm.Domain.Entities.Identity;

namespace ErpCrm.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(AppUser user, IList<string> roles, IList<string> permissions);
    string GenerateRefreshToken();
    Guid? GetUserIdFromExpiredToken(string token);
}
