using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using PlexRequests.Store.Models;

namespace PlexRequests.Store
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

        public async Task<PlexServer> Update(PlexServer server)
        {
            var filter = new FilterDefinitionBuilder<PlexServer>().Eq(x => x.Id, server.Id);
            var updatedServer = await Collection.FindOneAndReplaceAsync(filter, server);

            return updatedServer;
        }

        public async Task Delete(Guid id)
        {
            await Collection.DeleteOneAsync(x => x.Id == id);
        }

        public async Task<PlexServer> Get(Guid id)
        {
            var findCursor = await Collection.FindAsync(x => x.Id == id);
            return await findCursor.FirstOrDefaultAsync();
        }

        public async Task<List<PlexServer>> GetAll()
        {
            var users = await Collection.FindAsync(FilterDefinition<PlexServer>.Empty);
            return await users.ToListAsync();
        }
    }
}
