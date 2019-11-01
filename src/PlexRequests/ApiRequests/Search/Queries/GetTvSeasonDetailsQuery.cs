using MediatR;
using PlexRequests.ApiRequests.Search.Models;

namespace PlexRequests.ApiRequests.Search.Queries
{
    public class GetTvSeasonDetailsQuery : IRequest<TvSeasonDetailModel>
    {
        public GetTvSeasonDetailsQuery(int tvId, int seasonNumber)
        {
            TvId = tvId;
            SeasonNumber = seasonNumber;
        }

        public int TvId { get; set; }
        public int SeasonNumber { get; set; }
    }
}