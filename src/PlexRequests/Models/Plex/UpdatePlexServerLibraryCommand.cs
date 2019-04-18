using MediatR;
using Newtonsoft.Json;

namespace PlexRequests.Models.Plex
{
    public class UpdatePlexServerLibraryCommand : IRequest
    {
        [JsonIgnore]
        public string Key { get; set; }
        
        public bool IsEnabled { get; set; }
    }
}