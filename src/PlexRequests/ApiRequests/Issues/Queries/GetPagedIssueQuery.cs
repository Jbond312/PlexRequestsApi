using System.ComponentModel.DataAnnotations;
using MediatR;

namespace PlexRequests.ApiRequests.Issues.Queries
{
    public class GetPagedIssueQuery : IRequest<GetPagedIssueQueryResult>
    {
        [Range(1, int.MaxValue)]
        public int? Page { get; set; }
        [Range(1, int.MaxValue)]
        public int? PageSize { get; set; }
        public bool IncludeResolved { get; set; }
    }
}