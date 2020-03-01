using MediatR;
using Newtonsoft.Json;

namespace PlexRequests.Functions.Features.Plex.Commands
{
    public class UpdatePlexServerLibraryCommand : IRequest<ValidationContext>
    {
        [JsonIgnore]
        public string Key { get; set; }
        
        public bool IsEnabled { get; set; }
    }
}