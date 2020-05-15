using System.ComponentModel.DataAnnotations;
using MediatR;
using PlexRequests.DataAccess.Enums;

namespace PlexRequests.Functions.Features.Issues.Commands
{
    public class CreateIssueCommand : BaseRequest, IRequest<ValidationContext>
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int TheMovieDbId { get; set; }
        [Required]
        public PlexMediaTypes MediaType { get; set; }
        [Required]
        [MinLength(1)]
        public string Title { get; set; }
        [Required]
        [MinLength(1)]
        public string Description { get; set; }
    }
}