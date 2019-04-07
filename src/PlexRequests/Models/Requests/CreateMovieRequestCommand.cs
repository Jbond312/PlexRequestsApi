using MediatR;

namespace PlexRequests.Models.Requests
{
    public class CreateMovieRequestCommand : IRequest
    {
        public int TheMovieDbId { get; set; }
    }
}