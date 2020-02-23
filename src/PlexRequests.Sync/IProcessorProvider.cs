using PlexRequests.Sync.SyncProcessors;

namespace PlexRequests.Sync
{
    public interface IProcessorProvider
    {
        ISyncProcessor GetProcessor(string type);
    }
}
