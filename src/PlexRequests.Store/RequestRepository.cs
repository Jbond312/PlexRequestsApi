using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using PlexRequests.Store.Models;

namespace PlexRequests.Store
{
    public class RequestRepository : BaseRepository<Request>, IRequestRepository
    {
        public RequestRepository(string connectionString, string databaseName) : base(connectionString, databaseName, "Requests")
        {
        }

        public async Task Create(Request request)
        {
            await Collection.InsertOneAsync(request);
        }

        public async Task<List<Request>> GetMany(Expression<Func<Request, bool>> filter = null)
        {
            var cursor = await GetRequestsCursor(filter);
            return await cursor.ToListAsync();
        }
        
        public async Task<Request> GetOne(Expression<Func<Request, bool>> filter = null)
        {
            var cursor = await GetRequestsCursor(filter);
            return await cursor.FirstOrDefaultAsync();
        }

        public async Task Delete(Guid id)
        {
            await Collection.DeleteOneAsync(x => x.Id == id);
        }

        private async Task<IAsyncCursor<Request>> GetRequestsCursor(Expression<Func<Request, bool>> filter = null)
        {
            IAsyncCursor<Request> cursor;
            if (filter == null)
            {
                cursor = await Collection.FindAsync(FilterDefinition<Request>.Empty);
            }
            else
            {
                cursor = await Collection.FindAsync(filter);
            }

            return cursor;
        }
    }
}