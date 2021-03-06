using System.Threading;
using System.Threading.Tasks;
using MediatR;
using PlexRequests.Functions.Features.Search.Helpers;
using PlexRequests.Functions.Features.Search.Models;
using PlexRequests.TheMovieDb;

namespace PlexRequests.Functions.Features.Search.Queries
{
    public class GetTvSeasonDetailsQueryHandler : IRequestHandler<GetTvSeasonDetailsQuery, TvSeasonDetailModel>
    {
        private readonly ITheMovieDbService _theMovieDbService;
        private readonly ITvQueryHelper _tvQueryHelper;

        public GetTvSeasonDetailsQueryHandler(
            ITheMovieDbService theMovieDbService,
            ITvQueryHelper tvQueryHelper
            )
        {
            _theMovieDbService = theMovieDbService;
            _tvQueryHelper = tvQueryHelper;
        }

        public async Task<TvSeasonDetailModel> Handle(GetTvSeasonDetailsQuery request, CancellationToken cancellationToken)
        {
            var tvSeasonDetails = await _theMovieDbService.GetTvSeasonDetails(request.TvId, request.SeasonNumber);

            var tvSeasonDetailModel = await _tvQueryHelper.CreateSeasonDetailModel(request.TvId, tvSeasonDetails);

            return tvSeasonDetailModel;
        }
    }
}