using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRequests.Store.Models;

namespace PlexRequests.Store
{
    public class PlexMediaRepository: BaseRepository<PlexMediaItem>, IPlexMediaRepository
    {
        public PlexMediaRepository(string connectionString, string databaseName) : base(connectionString, databaseName, "PlexMediaItems")
        {
        }

        public async Task CreateMany(IEnumerable<PlexMediaItem> mediaItems)
        {
            await Collection.InsertManyAsync(mediaItems);
        }
    }
}
