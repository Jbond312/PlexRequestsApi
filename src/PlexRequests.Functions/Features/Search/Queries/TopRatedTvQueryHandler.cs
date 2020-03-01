using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using PlexRequests.Functions.Features.Search.Helpers;
using PlexRequests.Functions.Features.Search.Models;
using PlexRequests.TheMovieDb;

namespace PlexRequests.Functions.Features.Search.Queries
{
    public class TopRatedTvQueryHandler : IRequestHandler<TopRatedTvQuery, List<TvSearchModel>>
    {
        private readonly ITheMovieDbService _theMovieDbService;
        private readonly ITvQueryHelper _queryHelper;

        public TopRatedTvQueryHandler(
            ITheMovieDbService theMovieDbService,
            ITvQueryHelper queryHelper
        )
        {
            _theMovieDbService = theMovieDbService;
            _queryHelper = queryHelper;
        }

        public async Task<List<TvSearchModel>> Handle(TopRatedTvQuery request, CancellationToken cancellationToken)
        {
            var queryResults = await _theMovieDbService.TopRatedTv(request.Page);

            return await _queryHelper.CreateSearchModels(queryResults);
        }
    }
}
