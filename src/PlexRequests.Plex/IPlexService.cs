using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRequests.Store.Models;

namespace PlexRequests.Plex
{
    public interface IPlexService
    {
        Task<List<PlexServer>> GetServers();
        Task<PlexServer> GetServer(Guid id);
        Task<PlexServer> Create(PlexServer server);
        Task<PlexServer> Update(PlexServer server);
        Task DeleteServer(Guid id);
    }
}
