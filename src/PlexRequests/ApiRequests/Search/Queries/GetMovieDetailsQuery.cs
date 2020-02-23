using MediatR;
using PlexRequests.ApiRequests.Search.Models;

namespace PlexRequests.ApiRequests.Search.Queries
{
    public class GetMovieDetailsQuery : IRequest<MovieDetailModel>
    {
        public GetMovieDetailsQuery(int movieId)
        {
            MovieId = movieId;
        }

        public int MovieId { get; set; }
    }
}