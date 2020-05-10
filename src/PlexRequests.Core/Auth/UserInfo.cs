using System.Collections.Generic;

namespace PlexRequests.Core.Auth
{
    public class UserInfo
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public List<string> Roles { get; set; }
    }
}
