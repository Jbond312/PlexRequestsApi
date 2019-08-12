using System;
using System.ComponentModel.DataAnnotations;
using MediatR;
using Newtonsoft.Json;
using PlexRequests.Repository.Enums;

namespace PlexRequests.ApiRequests.Issues.Commands
{
    public class UpdateIssueCommand : IRequest
    {
        [JsonIgnore]
        public Guid Id { get; set; }
        [Required]
        public IssueStatuses Status { get; set; }
        [MinLength(1)]
        public string Resolution { get; set; }
    }
}