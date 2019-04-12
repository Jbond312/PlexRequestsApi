using MediatR;
using PlexRequests.Store.Enums;

namespace PlexRequests.Models.Requests
{
    public class GetPagedRequestQuery : IRequest<GetPagedRequestQueryResult>
    {
        public string Title { get; set; }
        public PlexMediaTypes? MediaType { get; set; }
        public bool? IsApproved { get; set; }
        public bool? IncludeCurrentUsersOnly { get; set; }
        public int? Page { get; set; }
        public int? PageSize { get; set; }
    }
}