using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;

namespace PlexRequests.DataAccess.Repositories
{
    public interface ITvRequestRepository
    {
        Task Add(TvRequestRow request);
        Task<List<TvRequestRow>> GetMany(Expression<Func<TvRequestRow, bool>> filter = null);
        Task<Paged<TvRequestRow>> GetPaged(string title, RequestStatuses? status, int? userId, int? page, int? pageSize);
        Task<TvRequestRow> GetOne(Expression<Func<TvRequestRow, bool>> filter);
        Task Delete(int id);
    }

    public class TvRequestRepository : BaseRepository, ITvRequestRepository
    {
        public TvRequestRepository(PlexRequestsDataContext dbContext) : base(dbContext)
        {
        }

        public async Task Add(TvRequestRow request)
        {
            await base.Add(request);
        }

        public async Task<List<TvRequestRow>> GetMany(Expression<Func<TvRequestRow, bool>> filter = null)
        {
            var query = DbContext.TvRequests.AsQueryable();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return await query.ToListAsync();
        }

        public async Task<Paged<TvRequestRow>> GetPaged(string title, RequestStatuses? status, int? userId, int? page, int? pageSize)
        {
            var query = DbContext.TvRequests.AsQueryable();

            if (!string.IsNullOrEmpty(title))
            {
                query = query.Where(x => x.Title.ToLower().Contains(title.ToLower()));
            }

            if (status != null)
            {
                query = query.Where(x => x.RequestStatus == status);
            }

            if (userId != null)
            {
                query = query.Where(x => x.TvRequestUsers.Any(tvru => tvru.UserId == userId));
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

            query = query.OrderByDescending(x => x.CreatedUtc)
                .Skip(pageSize.Value * page.Value)
                .Take(pageSize.Value);

            var items = await query.ToListAsync();

            return new Paged<TvRequestRow>
            {
                Page = page.Value + 1,
                PageSize = pageSize.Value,
                TotalPages = totalPages,
                Items = items
            };
        }

        public async Task<TvRequestRow> GetOne(Expression<Func<TvRequestRow, bool>> filter)
        {
            return await DbContext.TvRequests.FirstOrDefaultAsync(filter);
        }

        public async Task Delete(int id)
        {
            var tvRequestToDelete = await DbContext.TvRequests.FirstOrDefaultAsync(x => x.TvRequestId == id);

            if (tvRequestToDelete == null)
            {
                return;
            }

            DbContext.TvRequests.Remove(tvRequestToDelete);
        }
    }
}
