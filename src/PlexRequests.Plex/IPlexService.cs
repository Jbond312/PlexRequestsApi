using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;

namespace PlexRequests.Plex
{
    public interface IPlexService
    {
        Task<PlexServerRow> GetServer();
        Task AddServer(PlexServerRow server);
        Task<List<PlexMediaItemRow>> GetMediaItems(PlexMediaTypes mediaType);
        Task<PlexMediaItemRow> GetExistingMediaItemByAgent(PlexMediaTypes mediaType, AgentTypes agentType, string agentSourceId);
        Task CreateMany(List<PlexMediaItemRow> mediaItems);
        void DeleteAllMediaItems();
    }
}
