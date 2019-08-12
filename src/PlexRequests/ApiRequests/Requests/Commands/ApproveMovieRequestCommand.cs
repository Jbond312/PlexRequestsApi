using System;
using System.ComponentModel.DataAnnotations;
using MediatR;

namespace PlexRequests.ApiRequests.Requests.Commands
{
    public class ApproveMovieRequestCommand : IRequest
    {
        [Required]
        public Guid RequestId { get; }

        public ApproveMovieRequestCommand(Guid id)
        {
            RequestId = id;
        }

    }
}