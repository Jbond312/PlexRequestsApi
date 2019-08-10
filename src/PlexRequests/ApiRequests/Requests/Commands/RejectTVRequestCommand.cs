using System;
using System.Collections.Generic;
using MediatR;
using Newtonsoft.Json;

namespace PlexRequests.ApiRequests.Requests.Commands
{
    public class RejectTvRequestCommand : IRequest
    {
        [JsonIgnore]
        public Guid RequestId { get; set; }
        public string Comment { get; set; }
        public bool RejectAll { get; set; }
        public Dictionary<int, List<int>> EpisodesBySeason { get; set; }
    }
}