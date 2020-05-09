using System.Collections.Generic;
using PlexRequests.Functions.Features.Issues.Models.ListDetail;

namespace PlexRequests.Functions.Features.Issues.Queries
{
    public class GetPagedIssueQueryResult
    {
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int Page { get; set; }
        public List<IssueListDetailModel> Items { get; set; }
    }
}