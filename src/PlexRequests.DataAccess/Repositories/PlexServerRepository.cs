using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PlexRequests.DataAccess.Dtos;

namespace PlexRequests.DataAccess.Repositories
{
    public interface IPlexServerRepository
    {
        Task Add(PlexServerRow server);
        Task<PlexServerRow> Get();
    }

    public class PlexServerRepository : BaseRepository, IPlexServerRepository
    {
        public PlexServerRepository(PlexRequestsDataContext dbContext) : base(dbContext)
        {
        }

        public async Task Add(PlexServerRow server)
        {
            await base.Add(server);
        }

        public async Task<PlexServerRow> Get()
        {
            return await DbContext.PlexServers.FirstOrDefaultAsync();
        }
    }
}
