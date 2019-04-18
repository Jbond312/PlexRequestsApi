using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRequests.Repository.Enums;
using PlexRequests.Repository.Models;

namespace PlexRequests.Core.Services
{
    public interface ICompletionService
    {
        Task AutoCompleteRequests(Dictionary<RequestAgent, PlexMediaItem> agentsByPlexId, PlexMediaTypes mediaType);
    }
}