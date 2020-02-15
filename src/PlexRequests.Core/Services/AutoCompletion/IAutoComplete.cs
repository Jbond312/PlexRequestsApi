using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRequests.DataAccess;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;

namespace PlexRequests.Core.Services.AutoCompletion
{
    public interface IAutoComplete
    {
        Task AutoComplete(Dictionary<MediaAgent, PlexMediaItemRow> agentsByPlexId);
        PlexMediaTypes MediaType { get; }
    }
}
