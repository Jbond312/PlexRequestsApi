using Microsoft.AspNetCore.Authorization;
using PlexRequests.Core.Auth;

namespace PlexRequests.Functions.Attributes
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