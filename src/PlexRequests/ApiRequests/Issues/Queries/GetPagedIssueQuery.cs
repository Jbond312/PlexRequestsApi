using MediatR;

namespace PlexRequests.ApiRequests.Requests.Queries
{
    public class GetPagedIssueQuery : IRequest<GetPagedIssueQueryResult>
    {
        public int? Page { get; set; }
        public int? PageSize { get; set; }
        public bool IncludeResolved { get; set; }
    }
}