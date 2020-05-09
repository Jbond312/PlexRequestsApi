using System.ComponentModel.DataAnnotations;
using MediatR;
using Newtonsoft.Json;

namespace PlexRequests.Functions.Features.Issues.Commands
{
    public class CreateIssueCommentCommand : IRequest<ValidationContext>
    {
        [JsonIgnore]
        public int Id { get; set; }
        [Required]
        [MinLength(1)]
        public string Comment { get; set; }
    }
}