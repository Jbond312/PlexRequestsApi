using System.ComponentModel.DataAnnotations;
using MediatR;

namespace PlexRequests.Functions.Features.Requests.Commands
{
    public class DeleteMovieRequestCommand : IRequest<ValidationContext>
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int Id { get; set; }

        public DeleteMovieRequestCommand(int id)
        {
            Id = id;
        }
    }
}