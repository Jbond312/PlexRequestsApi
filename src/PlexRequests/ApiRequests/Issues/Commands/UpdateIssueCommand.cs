using System;
using MediatR;
using Newtonsoft.Json;
using PlexRequests.Repository.Enums;

namespace PlexRequests.ApiRequests.Requests.Commands
{
    public class UpdateIssueCommand : IRequest
    {
        [JsonIgnore]
        public Guid Id { get; set; }
        public IssueStatuses Status { get; set; }
        public string Resolution { get; set; }
    }
}