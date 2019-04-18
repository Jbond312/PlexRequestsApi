using System.Threading.Tasks;
using PlexRequests.Repository.Models;

namespace PlexRequests.Repository
{
    public interface IPlexServerRepository
    {
        Task<PlexServer> Create(PlexServer server);
        Task Update(PlexServer server);
        Task<PlexServer> Get();
    }
}
