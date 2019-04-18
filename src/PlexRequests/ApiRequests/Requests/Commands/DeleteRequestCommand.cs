using System;
using MediatR;

namespace PlexRequests.ApiRequests.Requests.Commands
{
    public class DeleteRequestCommand : IRequest
    {
        public Guid Id { get; set; }

        public DeleteRequestCommand(Guid id)
        {
            Id = id;
        }
    }
}