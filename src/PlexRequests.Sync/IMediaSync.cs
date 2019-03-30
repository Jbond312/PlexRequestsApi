using System.Threading.Tasks;
using PlexRequests.Store.Models;

namespace PlexRequests.Sync
{
    public interface IMediaSync
    {
        Task<SyncResult> SyncMedia(PlexServerLibrary library);
    }
}
