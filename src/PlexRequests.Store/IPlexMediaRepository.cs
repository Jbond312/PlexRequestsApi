using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRequests.Store.Enums;
using PlexRequests.Store.Models;

namespace PlexRequests.Store
{
    public interface IPlexMediaRepository
    {
        Task<List<PlexMediaItem>> GetAll(PlexMediaTypes? mediaType = null);
        Task CreateMany(IEnumerable<PlexMediaItem> mediaItems);
        Task Update(PlexMediaItem mediaItem);
        Task DeleteAll();
    }
}
