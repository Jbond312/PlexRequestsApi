using System.Threading.Tasks;
using MongoDB.Driver;
using PlexRequests.Repository.Models;

namespace PlexRequests.Repository
{
    public class PlexServerRepository : BaseRepository<PlexServer>, IPlexServerRepository
    {
        public PlexServerRepository(string connectionString, string databaseName) : base(connectionString, databaseName, "PlexServers")
        {
        }

        public async Task<PlexServer> Create(PlexServer server)
        {
            await Collection.InsertOneAsync(server);
            return server;
        }

        public async Task Update(PlexServer server)
        {
            var filter = new FilterDefinitionBuilder<PlexServer>().Eq(x => x.Id, server.Id);
            await Collection.FindOneAndReplaceAsync(filter, server);
        }

        public async Task<PlexServer> Get()
        {
            var findCursor = await Collection.FindAsync(FilterDefinition<PlexServer>.Empty);
            return await findCursor.FirstOrDefaultAsync();
        }
    }
}
