using System.Collections.Generic;
using PlexRequests.Functions.Features.Users.Models.Detail;

namespace PlexRequests.Functions.Features.Users.Queries
{
    public class GetManyUserQueryResult
    {
        public List<UserDetailModel> Users { get; set; }
    }
}
