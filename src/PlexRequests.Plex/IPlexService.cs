using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRequests.Store.Enums;
using PlexRequests.Store.Models;

namespace PlexRequests.Plex
{
    public interface IPlexService
    {
        Task<PlexServer> GetServer();
        Task<PlexServer> Create(PlexServer server);
        Task Update(PlexServer server);
        Task<List<PlexMediaItem>> GetMediaItems(PlexMediaTypes? mediaType);
        Task CreateMany(List<PlexMediaItem> mediaItems);
        Task UpdateMany(List<PlexMediaItem> mediaItems);
    }
}
