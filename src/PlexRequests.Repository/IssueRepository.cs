using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using PlexRequests.Repository.Models;

namespace PlexRequests.Repository
{
    public class IssuesRepository : BaseRepository<Issue>, IIssuesRepository
    {
        public IssuesRepository(string connectionString, string databaseName) : base(connectionString, databaseName, "Issues")
        {
        }

        public async Task Create(Issue issue)
        {
            await Collection.InsertOneAsync(issue);
        }

        public async Task Update(Issue issue)
        {
            await Collection.FindOneAndReplaceAsync(x => x.Id == issue.Id, issue);
        }

        public async Task<List<Issue>> GetMany(Expression<Func<Issue, bool>> filter = null)
        {
            var cursor = await GetRequestsCursor(filter);
            return await cursor.ToListAsync();
        }

        public async Task<Paged<Issue>> GetPaged(int? page, int? pageSize, Expression<Func<Issue, bool>> filter = null)
        {
            var query = Collection.AsQueryable();

            if (page == null)
            {
                page = 1;
            }

            if (pageSize == null)
            {
                pageSize = 10;
            }

            if (filter != null)
            {
                query = query.Where(filter);
            }

            var totalItems = await query.CountAsync();

            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            page -= 1;

            query = query.OrderByDescending(x => x.Created)
                 .Skip(pageSize.Value * page.Value)
                 .Take(pageSize.Value);

            var items = await query.ToListAsync();

            return new Paged<Issue>
            {
                Page = page.Value + 1,
                PageSize = pageSize.Value,
                TotalPages = totalPages,
                Items = items
            };
        }

        public async Task<Issue> GetOne(Expression<Func<Issue, bool>> filter = null)
        {
            var cursor = await GetRequestsCursor(filter);
            return await cursor.FirstOrDefaultAsync();
        }

        private async Task<IAsyncCursor<Issue>> GetRequestsCursor(Expression<Func<Issue, bool>> filter = null)
        {
            IAsyncCursor<Issue> cursor;
            if (filter == null)
            {
                cursor = await Collection.FindAsync(FilterDefinition<Issue>.Empty);
            }
            else
            {
                cursor = await Collection.FindAsync(filter);
            }

            return cursor;
        }
    }
}