using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using PlexRequests.Functions.Features.Search.Helpers;
using PlexRequests.Functions.Features.Search.Models;
using PlexRequests.TheMovieDb;

namespace PlexRequests.Functions.Features.Search.Queries
{
    public class TopRatedMovieQueryHandler : IRequestHandler<TopRatedMovieQuery, List<MovieSearchModel>>
    {
        private readonly ITheMovieDbService _theMovieDbService;
        private readonly IMovieQueryHelper _queryHelper;

        public TopRatedMovieQueryHandler(
            ITheMovieDbService theMovieDbService,
            IMovieQueryHelper queryHelper
        )
        {
            _theMovieDbService = theMovieDbService;
            _queryHelper = queryHelper;
        }

        public async Task<List<MovieSearchModel>> Handle(TopRatedMovieQuery request, CancellationToken cancellationToken)
        {
            var queryResults = await _theMovieDbService.TopRatedMovies(request.Page);

            return await _queryHelper.CreateSearchModels(queryResults);
        }
    }
}
