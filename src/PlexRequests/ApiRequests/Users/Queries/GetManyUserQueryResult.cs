using System.Collections.Generic;
using PlexRequests.ApiRequests.Users.Models.Detail;

namespace PlexRequests.ApiRequests.Users.Queries
{
    public class GetManyUserQueryResult
    {
        public List<UserDetailModel> Users { get; set; }
    }
}
