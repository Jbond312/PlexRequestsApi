using System.ComponentModel.DataAnnotations;
using MediatR;
using PlexRequests.Repository.Enums;

namespace PlexRequests.ApiRequests.Requests.Queries
{
    public class GetTvPagedRequestQuery : IRequest<GetTvPagedRequestQueryResult>
    {
        public string Title { get; set; }
        public RequestStatuses? Status { get; set; }
        public bool? IncludeCurrentUsersOnly { get; set; }
        [Range(1, int.MaxValue)]
        public int? Page { get; set; }
        [Range(1, int.MaxValue)]
        public int? PageSize { get; set; }
    }
}