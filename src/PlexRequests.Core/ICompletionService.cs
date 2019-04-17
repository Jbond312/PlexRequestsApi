using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRequests.Store.Enums;
using PlexRequests.Store.Models;

namespace PlexRequests.Core
{
    public interface ICompletionService
    {
        Task AutoCompleteRequests(Dictionary<RequestAgent, PlexMediaItem> agentsByPlexId, PlexMediaTypes mediaType);
    }
}