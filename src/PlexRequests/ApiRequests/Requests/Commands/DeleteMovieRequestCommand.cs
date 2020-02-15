using System.ComponentModel.DataAnnotations;
using MediatR;

namespace PlexRequests.ApiRequests.Requests.Commands
{
    public class DeleteMovieRequestCommand : IRequest
    {
        [Required]
        public int Id { get; set; }

        public DeleteMovieRequestCommand(int id)
        {
            Id = id;
        }
    }
}