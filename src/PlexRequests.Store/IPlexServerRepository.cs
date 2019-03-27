using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRequests.Store.Models;

namespace PlexRequests.Store
{
    public interface IPlexServerRepository
    {
        Task<PlexServer> Create(PlexServer server);
        Task<PlexServer> Update(PlexServer server);
        Task Delete(Guid id);
        Task<PlexServer> Get(Guid id);
        Task<List<PlexServer>> GetAll();
    }
}
