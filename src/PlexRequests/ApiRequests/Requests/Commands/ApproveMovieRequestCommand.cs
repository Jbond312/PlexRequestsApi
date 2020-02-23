using System.ComponentModel.DataAnnotations;
using MediatR;

namespace PlexRequests.ApiRequests.Requests.Commands
{
    public class ApproveMovieRequestCommand : IRequest
    {
        [Required]
        public int RequestId { get; }

        public ApproveMovieRequestCommand(int id)
        {
            RequestId = id;
        }

    }
}