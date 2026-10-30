

using SchoolPilot_IdentityServer.Data.Entities;

namespace SchoolPilot_IdentityServer.Infrastructure.Interfaces
{
    public interface ITokenService
    {
        Task<string> GenerateToken(ApplicationUser user);
        string GenerateRefreshToken();
    }
}
