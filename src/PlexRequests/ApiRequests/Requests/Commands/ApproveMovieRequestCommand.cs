using System;
using MediatR;

namespace PlexRequests.ApiRequests.Requests.Commands
{
    public class ApproveMovieRequestCommand : IRequest
    {
        public Guid RequestId { get; }

        public ApproveMovieRequestCommand(Guid id)
        {
            RequestId = id;
        }

    }
}