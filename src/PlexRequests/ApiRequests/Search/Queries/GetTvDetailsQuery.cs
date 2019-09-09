using MediatR;
using PlexRequests.ApiRequests.Search.Models;

namespace PlexRequests.ApiRequests.Search.Queries
{
    public class GetTvDetailsQuery : IRequest<TvDetailModel>
    {
        public GetTvDetailsQuery(int tvId)
        {
            TvId = tvId;
        }

        public int TvId { get; set; }
    }
}