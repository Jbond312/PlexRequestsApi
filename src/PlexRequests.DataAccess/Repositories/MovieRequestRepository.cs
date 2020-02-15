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
    public interface IMovieRequestRepository
    {
        Task Add(MovieRequestRow request);
        Task<List<MovieRequestRow>> GetMany(Expression<Func<MovieRequestRow, bool>> filter = null);
        Task<Paged<MovieRequestRow>> GetPaged(string title, RequestStatuses? status, int? userId, int? page,
            int? pageSize);
        Task<MovieRequestRow> GetOne(Expression<Func<MovieRequestRow, bool>> filter);
        Task<MovieRequestRow> GetOne(AgentTypes agentType, string agentSourceId);
        Task Delete(int id);
    }

    public class MovieRequestRepository : BaseRepository, IMovieRequestRepository
    {
        public MovieRequestRepository(PlexRequestsDataContext dbContext) : base(dbContext)
        {
        }

        public async Task Add(MovieRequestRow request)
        {
            await base.Add(request);
        }

        public async Task<List<MovieRequestRow>> GetMany(Expression<Func<MovieRequestRow, bool>> filter = null)
        {
            var query = DbContext.MovieRequests.AsQueryable();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return await query.ToListAsync();
        }

        public async Task<Paged<MovieRequestRow>> GetPaged(string title, RequestStatuses? status, int? userId, int? page, int? pageSize)
        {
            var query = DbContext.MovieRequests.AsQueryable();

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
                query = query.Where(x => x.UserId == userId);
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

            return new Paged<MovieRequestRow>
            {
                Page = page.Value + 1,
                PageSize = pageSize.Value,
                TotalPages = totalPages,
                Items = items
            };
        }

        public async Task<MovieRequestRow> GetOne(Expression<Func<MovieRequestRow, bool>> filter)
        {
            return await DbContext.MovieRequests.FirstOrDefaultAsync(filter);
        }

        public async Task<MovieRequestRow> GetOne(AgentTypes agentType, string agentSourceId)
        {
            return await DbContext.
                MovieRequests
                .Include(x => x.MovieRequestAgents)
                .Where(x => x.MovieRequestAgents.Any(mra => mra.AgentType == agentType && mra.AgentSourceId == agentSourceId)).
                FirstOrDefaultAsync();
        }

        public async Task Delete(int id)
        {
            var movieToDelete = await DbContext.MovieRequests.FirstOrDefaultAsync(x => x.MovieRequestId == id);

            if (movieToDelete == null)
            {
                return;
            }

            DbContext.MovieRequests.Remove(movieToDelete);
        }
    }
}
