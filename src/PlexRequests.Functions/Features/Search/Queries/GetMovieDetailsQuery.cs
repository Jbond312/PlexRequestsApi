using System.ComponentModel.DataAnnotations;
using MediatR;
using PlexRequests.Functions.Features.Search.Models;

namespace PlexRequests.Functions.Features.Search.Queries
{
    public class GetMovieDetailsQuery : IRequest<MovieDetailModel>
    {
        public GetMovieDetailsQuery(int movieId)
        {
            MovieId = movieId;
        }

        [Required]
        [Range(1, int.MaxValue)]
        public int MovieId { get; set; }
    }
}