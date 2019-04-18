using MediatR;

namespace PlexRequests.ApiRequests.Requests.Commands
{
    public class CreateMovieRequestCommand : IRequest
    {
        public int TheMovieDbId { get; set; }
    }
}