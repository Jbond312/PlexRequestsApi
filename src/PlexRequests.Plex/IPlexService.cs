using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using PlexRequests.Store.Models;

namespace PlexRequests.Plex
{
    public interface IPlexService
    {
        Task<PlexServer> GetServer();
        Task<PlexServer> Create(PlexServer server);
        Task Update(PlexServer server);
        Task<List<PlexMediaItem>> GetMediaItems(Expression<Func<PlexMediaItem, bool>> filter = null);
        Task<PlexMediaItem> GetOneMediaItem(Expression<Func<PlexMediaItem, bool>> filter = null);
        Task CreateMany(List<PlexMediaItem> mediaItems);
        Task UpdateMany(List<PlexMediaItem> mediaItems);
        Task DeleteAllMediaItems();
    }
}
