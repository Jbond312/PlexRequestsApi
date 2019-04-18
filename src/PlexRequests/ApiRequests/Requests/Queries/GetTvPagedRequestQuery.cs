using MediatR;
using PlexRequests.Repository.Enums;

namespace PlexRequests.ApiRequests.Requests.Queries
{
    public class GetTvPagedRequestQuery : IRequest<GetTvPagedRequestQueryResult>
    {
        public string Title { get; set; }
        public RequestStatuses? Status { get; set; }
        public bool? IncludeCurrentUsersOnly { get; set; }
        public int? Page { get; set; }
        public int? PageSize { get; set; }
    }
}