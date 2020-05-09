using System.ComponentModel.DataAnnotations;
using MediatR;

namespace PlexRequests.Functions.Features.Requests.Commands
{
    public class CreateMovieRequestCommand : IRequest<ValidationContext>
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int TheMovieDbId { get; set; }
    }
}