using System.Threading.Tasks;

namespace PlexRequests.Sync
{
    public interface IPlexSync
    {
        Task Synchronise(bool fullRefresh);
    }
}
