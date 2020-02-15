using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PlexRequests.DataAccess.Dtos;

namespace PlexRequests.DataAccess.Repositories
{
    public interface IPlexMediaItemRepository
    {
        Task<List<PlexMediaItemRow>> GetMany(Expression<Func<PlexMediaItemRow, bool>> filter = null);
        Task<PlexMediaItemRow> GetOne(Expression<Func<PlexMediaItemRow, bool>> filter);
        Task CreateMany(IEnumerable<PlexMediaItemRow> mediaItems);
        void DeleteAll();
    }

    public class PlexMediaItemRepository : BaseRepository, IPlexMediaItemRepository
    {
        public PlexMediaItemRepository(PlexRequestsDataContext dbContext) : base(dbContext)
        {
        }

        public async Task<List<PlexMediaItemRow>> GetMany(Expression<Func<PlexMediaItemRow, bool>> filter = null)
        {
            var query = DbContext.PlexMediaItems.AsQueryable();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return await query.ToListAsync();
        }

        public async Task<PlexMediaItemRow> GetOne(Expression<Func<PlexMediaItemRow, bool>> filter)
        {
            return await DbContext.PlexMediaItems.FirstOrDefaultAsync(filter);
        }

        public async Task CreateMany(IEnumerable<PlexMediaItemRow> mediaItems)
        {
            await DbContext.PlexMediaItems.AddRangeAsync(mediaItems);
        }

        public void DeleteAll()
        {
            foreach (var mediaItem in DbContext.PlexMediaItems)
            {
                DbContext.PlexMediaItems.Remove(mediaItem);
            }
        }
    }
}
