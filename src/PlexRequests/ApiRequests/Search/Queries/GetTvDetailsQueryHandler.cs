using System.Threading;
using System.Threading.Tasks;
using MediatR;
using PlexRequests.ApiRequests.Search.Helpers;
using PlexRequests.ApiRequests.Search.Models;
using PlexRequests.TheMovieDb;

namespace PlexRequests.ApiRequests.Search.Queries
{
    public class GetTvDetailsQueryHandler : IRequestHandler<GetTvDetailsQuery, TvDetailModel>
    {
        private readonly ITheMovieDbService _theMovieDbService;
        private readonly ITvQueryHelper _tvQueryHelper;

        public GetTvDetailsQueryHandler(
            ITheMovieDbService theMovieDbService,
            ITvQueryHelper tvQueryHelper
            )
        {
            _theMovieDbService = theMovieDbService;
            _tvQueryHelper = tvQueryHelper;
        }

        public async Task<TvDetailModel> Handle(GetTvDetailsQuery request, CancellationToken cancellationToken)
        {
            var tvDetails = await _theMovieDbService.GetTvDetails(request.TvId);

            var tvDetailModel = await _tvQueryHelper.CreateShowDetailModel(tvDetails);

            return tvDetailModel;
        }
    }
}