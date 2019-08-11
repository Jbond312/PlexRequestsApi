using System;
using MediatR;
using Newtonsoft.Json;

namespace PlexRequests.ApiRequests.Requests.Commands
{
    public class CreateIssueCommentCommand : IRequest
    {
        [JsonIgnore]
        public Guid Id { get; set; }
        public string Comment { get; set; }
    }
}