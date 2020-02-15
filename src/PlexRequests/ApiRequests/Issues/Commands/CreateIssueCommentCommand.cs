using System;
using System.ComponentModel.DataAnnotations;
using MediatR;
using Newtonsoft.Json;

namespace PlexRequests.ApiRequests.Issues.Commands
{
    public class CreateIssueCommentCommand : IRequest
    {
        [JsonIgnore]
        public int Id { get; set; }
        [Required]
        [MinLength(1)]
        public string Comment { get; set; }
    }
}