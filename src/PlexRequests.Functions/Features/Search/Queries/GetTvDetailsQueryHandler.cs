using System.Threading;
using System.Threading.Tasks;
using MediatR;
using PlexRequests.Functions.Features.Search.Helpers;
using PlexRequests.Functions.Features.Search.Models;
using PlexRequests.TheMovieDb;

namespace PlexRequests.Functions.Features.Search.Queries
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