using Microsoft.AspNetCore.Authorization;
using PlexRequests.Core;
using PlexRequests.Core.Auth;

namespace PlexRequests.Attributes
{
    public class AdminAttribute : AuthorizeAttribute
    {
        public AdminAttribute()
        {
            var roles = new[]
            {
                PlexRequestRoles.Admin
            };
            Roles = string.Join(",", roles);
        }
    }
}