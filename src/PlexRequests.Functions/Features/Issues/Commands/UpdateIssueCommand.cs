using System.ComponentModel.DataAnnotations;
using MediatR;
using Newtonsoft.Json;
using PlexRequests.DataAccess.Enums;

namespace PlexRequests.Functions.Features.Issues.Commands
{
    public class UpdateIssueCommand : IRequest<ValidationContext>
    {
        [JsonIgnore]
        public int Id { get; set; }
        [Required]
        public IssueStatuses Status { get; set; }
        [MinLength(1)]
        public string Outcome { get; set; }
    }
}