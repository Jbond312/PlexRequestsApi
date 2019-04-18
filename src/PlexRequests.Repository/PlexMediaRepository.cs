using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using PlexRequests.Repository.Models;
using MongoDB.Driver;

namespace PlexRequests.Repository
{
    public class PlexMediaRepository: BaseRepository<PlexMediaItem>, IPlexMediaRepository
    {
        public PlexMediaRepository(string connectionString, string databaseName) : base(connectionString, databaseName, "PlexMediaItems")
        {
        }

        public async Task<List<PlexMediaItem>> GetMany(Expression<Func<PlexMediaItem, bool>> filter = null)
        {
            var cursor = await GetRequestsCursor(filter);

            return await cursor.ToListAsync();
        }
        
        public async Task<PlexMediaItem> GetOne(Expression<Func<PlexMediaItem, bool>> filter = null)
        {
            var cursor = await GetRequestsCursor(filter);

            return await cursor.FirstOrDefaultAsync();
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

        public async Task DeleteAll()
        {
            await Collection.DeleteManyAsync(FilterDefinition<PlexMediaItem>.Empty);
        }
        
        private async Task<IAsyncCursor<PlexMediaItem>> GetRequestsCursor(Expression<Func<PlexMediaItem, bool>> filter = null)
        {
            IAsyncCursor<PlexMediaItem> cursor;
            if (filter == null)
            {
                cursor = await Collection.FindAsync(FilterDefinition<PlexMediaItem>.Empty);
            }
            else
            {
                cursor = await Collection.FindAsync(filter);
            }

            return cursor;
        }
    }
}
