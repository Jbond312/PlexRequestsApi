using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using PlexRequests.ApiRequests.Search.Helpers;
using PlexRequests.ApiRequests.Search.Models;
using PlexRequests.TheMovieDb;

namespace PlexRequests.ApiRequests.Search.Queries
{
    public class UpcomingMovieQueryHandler : IRequestHandler<UpcomingMovieQuery, List<MovieSearchModel>>
    {
        private readonly ITheMovieDbService _theMovieDbService;
        private readonly IMovieQueryHelper _queryHelper;

        public UpcomingMovieQueryHandler(
            ITheMovieDbService theMovieDbService,
            IMovieQueryHelper queryHelper
        )
        {
            _theMovieDbService = theMovieDbService;
            _queryHelper = queryHelper;
        }

        public async Task<List<MovieSearchModel>> Handle(UpcomingMovieQuery request, CancellationToken cancellationToken)
        {
            var queryResults = await _theMovieDbService.UpcomingMovies(request.Page);

            return await _queryHelper.CreateSearchModels(queryResults);
        }
    }
}
