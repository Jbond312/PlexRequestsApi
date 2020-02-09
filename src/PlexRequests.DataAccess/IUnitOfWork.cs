using System.Threading.Tasks;

namespace PlexRequests.DataAccess
{
    public interface IUnitOfWork
    {
        Task CommitAsync();
    }
}
