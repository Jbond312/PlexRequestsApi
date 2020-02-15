using System.Security.Claims;
using PlexRequests.DataAccess.Dtos;

namespace PlexRequests.Core.Services
{
    public interface ITokenService
    {
        string CreateToken(UserRow user);
        UserRefreshTokenRow CreateRefreshToken();
        ClaimsPrincipal GetPrincipalFromAccessToken(string accessToken);
    }
}
