using System.ComponentModel.DataAnnotations;
using MediatR;
using PlexRequests.DataAccess.Enums;

namespace PlexRequests.Functions.Features.Requests.Queries
{
    public class GetMoviePagedRequestQuery : BaseRequest, IRequest<GetMoviePagedRequestQueryResult>
    {
        [MinLength(1)]
        public string Title { get; set; }
        public RequestStatuses? Status { get; set; }
        public bool? IncludeCurrentUsersOnly { get; set; }
        [Range(1, int.MaxValue)]
        public int? Page { get; set; }
        [Range(1, int.MaxValue)]
        public int? PageSize { get; set; }
    }
}