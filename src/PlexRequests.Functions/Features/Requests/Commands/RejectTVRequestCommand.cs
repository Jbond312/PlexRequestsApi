using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MediatR;
using Newtonsoft.Json;

namespace PlexRequests.Functions.Features.Requests.Commands
{
    public class RejectTvRequestCommand : IRequest<ValidationContext>
    {
        [JsonIgnore]
        public int RequestId { get; set; }
        [Required]
        [MinLength(1)]
        public string Comment { get; set; }
        public bool RejectAll { get; set; }
        public Dictionary<int, List<int>> EpisodesBySeason { get; set; }
    }
}