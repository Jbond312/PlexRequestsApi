using System.Threading.Tasks;

namespace PlexRequests.DataAccess.Repositories
{
    public interface IBaseRepository
    {
        Task Add<T>(T entity) where T : class;
        Task SaveChanges();
    }

    public class BaseRepository : IBaseRepository
    {
        protected PlexRequestsDataContext DbContext;

        public BaseRepository(PlexRequestsDataContext dbContext)
        {
            DbContext = dbContext;
        }

        public async Task Add<T>(T entity) where T : class
        {
            await DbContext.Set<T>().AddAsync(entity);
        }

        public async Task SaveChanges()
        {
            await DbContext.SaveChangesAsync();
        }
    }
}
