using System.ComponentModel.DataAnnotations;
using MediatR;

namespace PlexRequests.ApiRequests.Requests.Commands
{
    public class DeleteTvRequestCommand : IRequest
    {
        [Required]
        public int Id { get; set; }

        public DeleteTvRequestCommand(int id)
        {
            Id = id;
        }
    }
}