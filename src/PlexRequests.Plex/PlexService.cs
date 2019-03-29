using System.Threading.Tasks;
using PlexRequests.Store;
using PlexRequests.Store.Models;

namespace PlexRequests.Plex
{
    public class PlexService : IPlexService
    {
        private readonly IPlexServerRepository _plexServerRepository;

        public PlexService(IPlexServerRepository plexServerRepository)
        {
            _plexServerRepository = plexServerRepository;
        }

        public async Task<PlexServer> GetServer()
        {
            return await _plexServerRepository.Get();
        }

        public async Task<PlexServer> Create(PlexServer server)
        {
            return await _plexServerRepository.Create(server);
        }

        public async Task Update(PlexServer server)
        {
            await _plexServerRepository.Update(server);
        }
    }
}
