using Microsoft.AspNetCore.Authorization;
using PlexRequests.Helpers;

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