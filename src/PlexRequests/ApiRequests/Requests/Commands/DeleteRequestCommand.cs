using System;
using System.ComponentModel.DataAnnotations;
using MediatR;

namespace PlexRequests.ApiRequests.Requests.Commands
{
    public class DeleteRequestCommand : IRequest
    {
        [Required]
        public Guid Id { get; set; }

        public DeleteRequestCommand(Guid id)
        {
            Id = id;
        }
    }
}