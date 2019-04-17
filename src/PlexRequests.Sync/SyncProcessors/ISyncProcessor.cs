using System.Threading.Tasks;
using PlexRequests.Plex.Models;
using PlexRequests.Store.Enums;

namespace PlexRequests.Sync.SyncProcessors
{
    public interface ISyncProcessor
    {
        Task<SyncResult> Synchronise(PlexMediaContainer libraryContainer, bool fullRefresh, string authToken, string plexUri, string machineIdentifier);
        PlexMediaTypes Type { get; }
    }
}
