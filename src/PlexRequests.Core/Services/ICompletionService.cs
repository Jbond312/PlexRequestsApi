using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRequests.DataAccess;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;

namespace PlexRequests.Core.Services
{
    public interface ICompletionService
    {
        Task AutoCompleteRequests(Dictionary<MediaAgent, PlexMediaItemRow> agentsByPlexId, PlexMediaTypes mediaType);
    }
}