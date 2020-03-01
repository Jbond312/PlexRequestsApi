using System.ComponentModel.DataAnnotations;
using MediatR;

namespace PlexRequests.Functions.Features.Requests.Commands
{
    public class DeleteMovieRequestCommand : IRequest<ValidationContext>
    {
        [Required]
        public int Id { get; set; }

        public DeleteMovieRequestCommand(int id)
        {
            Id = id;
        }
    }
}