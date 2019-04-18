using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRequests.Repository.Enums;
using PlexRequests.Repository.Models;

namespace PlexRequests.Plex
{
    public interface IPlexService
    {
        Task<PlexServer> GetServer();
        Task<PlexServer> Create(PlexServer server);
        Task Update(PlexServer server);
        Task<List<PlexMediaItem>> GetMediaItems(PlexMediaTypes mediaType);
        Task<PlexMediaItem> GetExistingMediaItemByAgent(PlexMediaTypes mediaType, AgentTypes agentType, string agentSourceId);
        Task CreateMany(List<PlexMediaItem> mediaItems);
        Task UpdateMany(List<PlexMediaItem> mediaItems);
        Task DeleteAllMediaItems();
    }
}
