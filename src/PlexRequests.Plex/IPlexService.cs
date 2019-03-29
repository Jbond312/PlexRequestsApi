using System.Threading.Tasks;
using PlexRequests.Store.Models;

namespace PlexRequests.Plex
{
    public interface IPlexService
    {
        Task<PlexServer> GetServer();
        Task<PlexServer> Create(PlexServer server);
        Task Update(PlexServer server);
    }
}
