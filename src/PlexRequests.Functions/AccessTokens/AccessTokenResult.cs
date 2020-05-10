using System;
using System.Linq;
using System.Security.Claims;
using PlexRequests.Core.Auth;

namespace PlexRequests.Functions.AccessTokens
{
    public sealed class AccessTokenResult
    {
        public ClaimsPrincipal Principal { get; private set; }
        public UserInfo UserInfo { get; set; }

        public AccessTokenStatus Status { get; private set; }

        public Exception Exception { get; private set; }

        public bool IsValid => Status == AccessTokenStatus.Valid;

        public static AccessTokenResult Success(ClaimsPrincipal principal)
        {
            var userNameClaim = principal.Claims.First(x => x.Type == ClaimTypes.Name);
            var userIdClaim = principal.Claims.First(x => x.Type == ClaimTypes.NameIdentifier);
            var roles = principal.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).ToList();

            var userInfo = new UserInfo
            {
                Username = userNameClaim.Value,
                UserId = int.Parse(userIdClaim.Value),
                Roles = roles
            };

            return new AccessTokenResult
            {
                Principal = principal,
                Status = AccessTokenStatus.Valid,
                UserInfo = userInfo
            };
        }
        
        public static AccessTokenResult Expired()
        {
            return new AccessTokenResult
            {
                Status = AccessTokenStatus.Expired
            };
        }

        public static AccessTokenResult Error(Exception ex)
        {
            return new AccessTokenResult
            {
                Status = AccessTokenStatus.Error,
                Exception = ex
            };
        }

        public static AccessTokenResult NoToken()
        {
            return new AccessTokenResult
            {
                Status = AccessTokenStatus.NoToken
            };
        }

        public static AccessTokenResult InsufficientPermissions(ClaimsPrincipal principal)
        {
            return new AccessTokenResult
            {
                Principal = principal,
                Status = AccessTokenStatus.InsufficientPermissions
            };
        }
    }
}
