using System.Threading.Tasks;

namespace PlexRequests.DataAccess.Repositories
{
    public interface IBaseRepository
    {
        Task SaveChanges();
    }
}
