using MediatR;
using PlexRequests.Repository.Enums;

namespace PlexRequests.ApiRequests.Requests.Queries
{
    public class GetMoviePagedRequestQuery : IRequest<GetMoviePagedRequestQueryResult>
    {
        public string Title { get; set; }
        public RequestStatuses? Status { get; set; }
        public bool? IncludeCurrentUsersOnly { get; set; }
        public int? Page { get; set; }
        public int? PageSize { get; set; }
    }
}