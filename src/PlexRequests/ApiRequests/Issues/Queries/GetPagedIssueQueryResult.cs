using System.Collections.Generic;
using PlexRequests.ApiRequests.Requests.DTOs;

namespace PlexRequests.ApiRequests.Requests.Queries
{
    public class GetPagedIssueQueryResult
    {
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int Page { get; set; }
        public List<IssueListDetailModel> Items { get; set; }
    }
}