using System.Collections.Generic;
using PlexRequests.ApiRequests.Requests.DTOs.Detail;

namespace PlexRequests.ApiRequests.Requests.Queries
{
    public class GetMoviePagedRequestQueryResult
    {
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int Page { get; set; }
        public List<MovieRequestDetailModel> Items { get; set; }
    }
}