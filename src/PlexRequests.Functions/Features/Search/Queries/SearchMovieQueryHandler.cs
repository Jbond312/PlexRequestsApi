using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using PlexRequests.Functions.Features.Search.Helpers;
using PlexRequests.Functions.Features.Search.Models;
using PlexRequests.TheMovieDb;

namespace PlexRequests.Functions.Features.Search.Queries
{
    public class SearchMovieQueryHandler : IRequestHandler<SearchMovieQuery, List<MovieSearchModel>>
    {
        private readonly ITheMovieDbService _theMovieDbService;
        private readonly IMovieQueryHelper _queryHelper;

        public SearchMovieQueryHandler(
            ITheMovieDbService theMovieDbService,
            IMovieQueryHelper queryHelper
        )
        {
            _theMovieDbService = theMovieDbService;
            _queryHelper = queryHelper;
        }

        public async Task<List<MovieSearchModel>> Handle(SearchMovieQuery request, CancellationToken cancellationToken)
        {
            var queryResults = await _theMovieDbService.SearchMovies(request.Query, request.Year, request.Page);

            return await _queryHelper.CreateSearchModels(queryResults);
        }
    }
}
