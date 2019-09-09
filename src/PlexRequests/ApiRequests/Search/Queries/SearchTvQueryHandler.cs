using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using PlexRequests.ApiRequests.Search.Helpers;
using PlexRequests.ApiRequests.Search.Models;
using PlexRequests.TheMovieDb;

namespace PlexRequests.ApiRequests.Search.Queries
{
    public class SearchTvQueryHandler : IRequestHandler<SearchTvQuery, List<TvSearchModel>>
    {
        private readonly ITheMovieDbService _theMovieDbService;
        private readonly ITvQueryHelper _queryHelper;

        public SearchTvQueryHandler(
            ITheMovieDbService theMovieDbService,
            ITvQueryHelper queryHelper
            )
        {
            _theMovieDbService = theMovieDbService;
            _queryHelper = queryHelper;
        }

        public async Task<List<TvSearchModel>> Handle(SearchTvQuery request, CancellationToken cancellationToken)
        {
            var queryResults = await _theMovieDbService.SearchTv(request.Query, request.Page);

            return await _queryHelper.CreateSearchModels(queryResults);
        }
    }
}