using System;
using System.Collections.Generic;
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

        public async Task<List<PlexServer>> GetServers()
        {
            return await _plexServerRepository.GetAll();
        }

        public async Task<PlexServer> GetServer(Guid id)
        {
            return await _plexServerRepository.Get(id);
        }

        public async Task<PlexServer> Create(PlexServer server)
        {
            return await _plexServerRepository.Create(server);
        }

        public async Task<PlexServer> Update(PlexServer server)
        {
            return await _plexServerRepository.Update(server);
        }

        public async Task DeleteServer(Guid id)
        {
            await _plexServerRepository.Delete(id);
        }
    }
}
