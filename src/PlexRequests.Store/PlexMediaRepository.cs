using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRequests.Store.Models;
using MongoDB.Driver;
using PlexRequests.Store.Enums;

namespace PlexRequests.Store
{
    public class PlexMediaRepository: BaseRepository<PlexMediaItem>, IPlexMediaRepository
    {
        public PlexMediaRepository(string connectionString, string databaseName) : base(connectionString, databaseName, "PlexMediaItems")
        {
        }

        public async Task<List<PlexMediaItem>> GetAll(PlexMediaTypes? mediaType = null)
        {
            var users = await Collection.FindAsync(FilterDefinition<PlexMediaItem>.Empty);
            return await users.ToListAsync();
        }

        public async Task CreateMany(IEnumerable<PlexMediaItem> mediaItems)
        {
            await Collection.InsertManyAsync(mediaItems);
        }

        public async Task Update(PlexMediaItem mediaItem)
        {
            var filter = new FilterDefinitionBuilder<PlexMediaItem>().Eq(x => x.Id, mediaItem.Id);
            await Collection.FindOneAndReplaceAsync(filter, mediaItem);
        }
    }
}
