using System;
using MediatR;

namespace PlexRequests.Models.Requests
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