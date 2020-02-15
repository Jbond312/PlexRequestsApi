using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace PlexRequests.Core.Helpers
{
    public class ClaimsPrincipalAccessor : IClaimsPrincipalAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClaimsPrincipalAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string Username
        {
            get
            {
                var userNameClaim = _httpContextAccessor.HttpContext.User.Claims.First(x => x.Type == ClaimTypes.Name);
                return userNameClaim.Value;
            }
        }

        public int UserId
        {
            get
            {
                var userIdClaim = _httpContextAccessor.HttpContext.User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier);
                return int.Parse(userIdClaim.Value);
            }
        }
    }
}