using System.Security.Claims;
using PlexRequests.Repository.Models;

namespace PlexRequests.Core.Services
{
    public interface ITokenService
    {
        string CreateToken(User user);
        RefreshToken CreateRefreshToken();
        ClaimsPrincipal GetPrincipalFromAccessToken(string accessToken);
    }
}
