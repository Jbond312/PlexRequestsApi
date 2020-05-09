using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using PlexRequests.Functions.Features.Search.Helpers;
using PlexRequests.Functions.Features.Search.Models;
using PlexRequests.TheMovieDb;

namespace PlexRequests.Functions.Features.Search.Queries
{
    public class PopularTvQueryHandler : IRequestHandler<PopularTvQuery, List<TvSearchModel>>
    {
        private readonly ITheMovieDbService _theMovieDbService;
        private readonly ITvQueryHelper _queryHelper;

        public PopularTvQueryHandler(
            ITheMovieDbService theMovieDbService,
            ITvQueryHelper queryHelper
        )
        {
            _theMovieDbService = theMovieDbService;
            _queryHelper = queryHelper;
        }

        public async Task<List<TvSearchModel>> Handle(PopularTvQuery request, CancellationToken cancellationToken)
        {
            var queryResults = await _theMovieDbService.PopularTv(request.Page);

            return await _queryHelper.CreateSearchModels(queryResults);
        }
    }
}
