using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRequests.Repository.Enums;
using PlexRequests.Repository.Models;

namespace PlexRequests.Core.Services.AutoCompletion
{
    public interface IAutoComplete
    {
        Task AutoComplete(Dictionary<MediaAgent, PlexMediaItem> agentsByPlexId);
        PlexMediaTypes MediaType { get; }
    }
}
