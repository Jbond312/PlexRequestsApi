using System.Collections.Generic;
using PlexRequests.Functions.Features.Requests.Models.Detail;

namespace PlexRequests.Functions.Features.Requests.Queries
{
    public class GetMoviePagedRequestQueryResult
    {
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int Page { get; set; }
        public List<MovieRequestDetailModel> Items { get; set; }
    }
}