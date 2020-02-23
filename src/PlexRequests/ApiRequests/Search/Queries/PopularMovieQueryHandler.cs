using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using PlexRequests.ApiRequests.Search.Helpers;
using PlexRequests.ApiRequests.Search.Models;
using PlexRequests.TheMovieDb;

namespace PlexRequests.ApiRequests.Search.Queries
{
    public class PopularMovieQueryHandler : IRequestHandler<PopularMovieQuery, List<MovieSearchModel>>
    {
        private readonly ITheMovieDbService _theMovieDbService;
        private readonly IMovieQueryHelper _queryHelper;

        public PopularMovieQueryHandler(
            ITheMovieDbService theMovieDbService,
            IMovieQueryHelper queryHelper
        )
        {
            _theMovieDbService = theMovieDbService;
            _queryHelper = queryHelper;
        }

        public async Task<List<MovieSearchModel>> Handle(PopularMovieQuery request, CancellationToken cancellationToken)
        {
            var queryResults = await _theMovieDbService.PopularMovies(request.Page);

            return await _queryHelper.CreateSearchModels(queryResults);
        }
    }
}
