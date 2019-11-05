using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using PlexRequests.Repository.Enums;
using PlexRequests.Repository.Models;

namespace PlexRequests.Repository
{
    public class TvRequestRepository : BaseRepository<TvRequest>, ITvRequestRepository
    {
        public TvRequestRepository(string connectionString, string databaseName) : base(connectionString, databaseName, "TvRequests")
        {
        }

        public async Task Create(TvRequest request)
        {
            await Collection.InsertOneAsync(request);
        }

        public async Task Update(TvRequest request)
        {
            await Collection.FindOneAndReplaceAsync(x => x.Id == request.Id, request);
        }

        public async Task<List<TvRequest>> GetMany(Expression<Func<TvRequest, bool>> filter = null)
        {
            var cursor = await GetRequestsCursor(filter);
            return await cursor.ToListAsync();
        }

        public async Task<List<TvRequest>> GetManyIn<TField>(Expression<Func<TvRequest, TField>> filter, List<TField> values)
        {
            var fBuilder = Builders<TvRequest>.Filter.In(filter, values);
            var cursor = await Collection.FindAsync(fBuilder);
            return await cursor.ToListAsync();
        }

        public async Task<Paged<TvRequest>> GetPaged(string title, RequestStatuses? status, Guid? userId, int? page, int? pageSize)
        {
            var query = Collection.AsQueryable();

            if (!string.IsNullOrEmpty(title))
            {
                query = query.Where(x => x.Title.ToLower().Contains(title.ToLower()));
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

            return new Paged<TvRequest>
            {
                Page = page.Value + 1,
                PageSize = pageSize.Value,
                TotalPages = totalPages,
                Items = items
            };
        }

        public async Task<TvRequest> GetOne(Expression<Func<TvRequest, bool>> filter = null)
        {
            var cursor = await GetRequestsCursor(filter);
            return await cursor.FirstOrDefaultAsync();
        }

        public async Task Delete(Guid id)
        {
            await Collection.DeleteOneAsync(x => x.Id == id);
        }

        private async Task<IAsyncCursor<TvRequest>> GetRequestsCursor(Expression<Func<TvRequest, bool>> filter = null)
        {
            IAsyncCursor<TvRequest> cursor;
            if (filter == null)
            {
                cursor = await Collection.FindAsync(FilterDefinition<TvRequest>.Empty);
            }
            else
            {
                cursor = await Collection.FindAsync(filter);
            }

            return cursor;
        }
    }
}