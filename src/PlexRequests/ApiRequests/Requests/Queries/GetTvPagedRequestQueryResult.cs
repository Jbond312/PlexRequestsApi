using System.Collections.Generic;
using PlexRequests.ApiRequests.Requests.Models.Detail;

namespace PlexRequests.ApiRequests.Requests.Queries
{
    public class GetTvPagedRequestQueryResult
    {
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int Page { get; set; }
        public List<TvRequestDetailModel> Items { get; set; }
    }
}