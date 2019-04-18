using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using PlexRequests.Store.Enums;
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

        public async Task Update(Request request)
        {
            await Collection.FindOneAndReplaceAsync(x => x.Id == request.Id, request);
        }

        public async Task<List<Request>> GetMany(Expression<Func<Request, bool>> filter = null)
        {
            var cursor = await GetRequestsCursor(filter);
            return await cursor.ToListAsync();
        }

        public async Task<Paged<Request>> GetPaged(string title, PlexMediaTypes? mediaType, RequestStatuses? status, Guid? userId, int? page, int? pageSize)
        {
            var query = Collection.AsQueryable();

            if (!string.IsNullOrEmpty(title))
            {
                query = query.Where(x => x.Title.ToLower().Contains(title.ToLower()));
            }
            
            if (mediaType != null)
            {
                query = query.Where(x => x.MediaType == mediaType);
            }

            if (status != null)
            {
                query = query.Where(x => x.Status == status);
            }

            if (userId != null)
            {
                query = query.Where(x => x.RequestedByUserId == userId);
            }

            if (page == null)
            {
                page = 1;
            }

            if (pageSize == null)
            {
                pageSize = 10;
            }

            var totalItems = await query.CountAsync();

            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            page -= 1;

            query = query.OrderByDescending(x => x.Created)
                 .Skip(pageSize.Value * page.Value)
                 .Take(pageSize.Value);

            var items = await query.ToListAsync();

            return new Paged<Request>
            {
                Page = page.Value + 1,
                PageSize = pageSize.Value,
                TotalPages = totalPages,
                Items = items
            };
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