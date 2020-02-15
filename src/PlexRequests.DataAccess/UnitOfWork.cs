using System.Threading.Tasks;

namespace PlexRequests.DataAccess
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly PlexRequestsDataContext _dbContext;

        public UnitOfWork(PlexRequestsDataContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task CommitAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
