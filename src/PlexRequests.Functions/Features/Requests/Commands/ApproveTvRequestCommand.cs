using System.Collections.Generic;
using MediatR;
using Newtonsoft.Json;

namespace PlexRequests.Functions.Features.Requests.Commands
{
    public class ApproveTvRequestCommand : IRequest<ValidationContext>
    {
        [JsonIgnore]
        public int RequestId { get; set; }
        public bool ApproveAll { get; set; }
        public Dictionary<int, List<int>> EpisodesBySeason { get; set; }
    }
}