using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using PlexRequests.Store.Models;

namespace PlexRequests.Store
{
    public interface IPlexMediaRepository
    {
        Task<List<PlexMediaItem>> GetMany(Expression<Func<PlexMediaItem, bool>> filter = null);
        Task<PlexMediaItem> GetOne(Expression<Func<PlexMediaItem, bool>> filter = null);
        Task CreateMany(IEnumerable<PlexMediaItem> mediaItems);
        Task Update(PlexMediaItem mediaItem);
        Task DeleteAll();
    }
}
