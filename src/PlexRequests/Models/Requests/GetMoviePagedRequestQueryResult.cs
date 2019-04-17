using System.Collections.Generic;
using PlexRequests.Models.SubModels.Detail;

namespace PlexRequests.Models.Requests
{
    public class GetMoviePagedRequestQueryResult
    {
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int Page { get; set; }
        public List<MovieRequestDetailModel> Items { get; set; }
    }
}