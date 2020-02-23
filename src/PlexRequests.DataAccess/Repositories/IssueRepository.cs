using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PlexRequests.DataAccess.Dtos;

namespace PlexRequests.DataAccess.Repositories
{
    public interface IIssuesRepository
    {
        Task Add(IssueRow issue);
        Task<List<IssueRow>> GetMany(Expression<Func<IssueRow, bool>> filter = null);
        Task<Paged<IssueRow>> GetPaged(int? page, int? pageSize, Expression<Func<IssueRow, bool>> filter = null);
        Task<IssueRow> GetOne(Expression<Func<IssueRow, bool>> filter);
    }

    public class IssuesRepository : BaseRepository, IIssuesRepository
    {
        public IssuesRepository(PlexRequestsDataContext dbContext) : base(dbContext)
        {
        }

        public async Task Add(IssueRow issue)
        {
            await base.Add(issue);
        }

        public async Task<List<IssueRow>> GetMany(Expression<Func<IssueRow, bool>> filter = null)
        {
            var query = DbContext.Issues.AsQueryable();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return await query.ToListAsync();
        }

        public async Task<Paged<IssueRow>> GetPaged(int? page, int? pageSize, Expression<Func<IssueRow, bool>> filter = null)
        {
            if (page == null)
            {
                page = 1;
            }

            if (pageSize == null)
            {
                pageSize = 10;
            }

            var query = DbContext.Issues.AsQueryable();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            var totalItems = await query.CountAsync();

            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            page -= 1;

            query = query.OrderByDescending(x => x.CreatedUtc)
                .Skip(pageSize.Value * page.Value)
                .Take(pageSize.Value);

            var items = await query.ToListAsync();

            return new Paged<IssueRow>
            {
                Page = page.Value + 1,
                PageSize = pageSize.Value,
                TotalPages = totalPages,
                Items = items
            };
        }

        public async Task<IssueRow> GetOne(Expression<Func<IssueRow, bool>> filter)
        {
            return await DbContext.Issues.Where(filter).FirstOrDefaultAsync();
        }
    }
}
