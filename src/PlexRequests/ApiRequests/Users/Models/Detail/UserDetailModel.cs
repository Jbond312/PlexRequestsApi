using System;
using System.Collections.Generic;

namespace PlexRequests.ApiRequests.Users.Models.Detail
{
    public class UserDetailModel
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public bool IsDisabled { get; set; }
        public DateTime LastLogin { get; set; }
        public List<string> Roles { get; set; }
    }
}
