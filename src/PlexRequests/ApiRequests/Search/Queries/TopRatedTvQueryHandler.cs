using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using PlexRequests.ApiRequests.Search.Helpers;
using PlexRequests.ApiRequests.Search.Models;
using PlexRequests.TheMovieDb;

namespace PlexRequests.ApiRequests.Search.Queries
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
