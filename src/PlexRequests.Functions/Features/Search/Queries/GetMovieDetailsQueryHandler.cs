using System.Threading;
using System.Threading.Tasks;
using MediatR;
using PlexRequests.Functions.Features.Search.Helpers;
using PlexRequests.Functions.Features.Search.Models;
using PlexRequests.TheMovieDb;

namespace PlexRequests.Functions.Features.Search.Queries
{
    public class GetMovieDetailsQueryHandler : IRequestHandler<GetMovieDetailsQuery, MovieDetailModel>
    {
        private readonly ITheMovieDbService _theMovieDbService;
        private readonly IMovieQueryHelper _movieQueryHelper;

        public GetMovieDetailsQueryHandler(
            ITheMovieDbService theMovieDbService,
            IMovieQueryHelper movieQueryHelper
            )
        {
            _theMovieDbService = theMovieDbService;
            _movieQueryHelper = movieQueryHelper;
        }

        public async Task<MovieDetailModel> Handle(GetMovieDetailsQuery request, CancellationToken cancellationToken)
        {
            var movieDetails = await _theMovieDbService.GetMovieDetails(request.MovieId);

            var movieDetailModel = await _movieQueryHelper.CreateDetailModel(movieDetails);

            return movieDetailModel;
        }
    }
}