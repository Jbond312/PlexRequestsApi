using Microsoft.AspNetCore.Http;

namespace PlexRequests.Functions.AccessTokens
{
    public interface IAccessTokenProvider
    {
        AccessTokenResult ValidateToken(HttpRequest request, string requiredRole = null);
    }
}
