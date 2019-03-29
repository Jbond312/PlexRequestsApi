using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRequests.Store.Models;

namespace PlexRequests.Store
{
    public interface IPlexMediaRepository
    {
        Task CreateMany(IEnumerable<PlexMediaItem> mediaItems);
    }
}
