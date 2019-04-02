using System.Threading.Tasks;
using PlexRequests.Store.Models;

namespace PlexRequests.Store
{
    public interface IPlexServerRepository
    {
        Task<PlexServer> Create(PlexServer server);
        Task Update(PlexServer server);
        Task<PlexServer> Get();
    }
}
