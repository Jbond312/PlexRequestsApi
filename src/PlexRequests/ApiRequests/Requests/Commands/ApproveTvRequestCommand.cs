using System;
using System.Collections.Generic;
using MediatR;
using Newtonsoft.Json;

namespace PlexRequests.ApiRequests.Requests.Commands
{
    public class ApproveTvRequestCommand : IRequest
    {
        [JsonIgnore]
        public Guid RequestId { get; set; }
        public bool ApproveAll { get; set; }
        public Dictionary<int, List<int>> EpisodesBySeason { get; set; }
    }
}