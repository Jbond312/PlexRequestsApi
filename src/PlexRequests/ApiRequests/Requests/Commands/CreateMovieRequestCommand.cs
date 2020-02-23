using System.ComponentModel.DataAnnotations;
using MediatR;

namespace PlexRequests.ApiRequests.Requests.Commands
{
    public class CreateMovieRequestCommand : IRequest
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int TheMovieDbId { get; set; }
    }
}