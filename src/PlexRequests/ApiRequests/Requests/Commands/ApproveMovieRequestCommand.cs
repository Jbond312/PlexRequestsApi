using System.ComponentModel.DataAnnotations;
using MediatR;

namespace PlexRequests.ApiRequests.Requests.Commands
{
    public class ApproveMovieRequestCommand : IRequest<ValidationContext>
    {
        [Required]
        public int RequestId { get; }

        public ApproveMovieRequestCommand(int id)
        {
            RequestId = id;
        }

    }
}