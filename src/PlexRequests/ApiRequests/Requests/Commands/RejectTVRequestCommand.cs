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

        public RejectTvRequestCommand(Guid id, string comment, Dictionary<int, List<int>> episodesBySeason = null)
        {
            RequestId = id;
            Comment = comment;
            EpisodesBySeason = episodesBySeason ?? new Dictionary<int, List<int>>();
        }
    }
}