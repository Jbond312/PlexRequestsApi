using System.Threading.Tasks;

namespace PlexRequests.DataAccess.Repositories
{
    public class BaseRepository : IBaseRepository
    {
        protected PlexRequestsDataContext DbContext;

        public BaseRepository(PlexRequestsDataContext dbContext)
        {
            DbContext = dbContext;
        }

        public async Task SaveChanges()
        {
            await DbContext.SaveChangesAsync();
        }
    }
}
