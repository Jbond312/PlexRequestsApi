using PlexRequests.Plex.Models;

namespace PlexRequests.Sync.UnitTests.SyncProcessors
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class SyncRequest
    {
        // ReSharper disable UnusedAutoPropertyAccessor.Local
        public PlexMediaContainer LibraryContainer { get; set; }
        public bool FullRefresh { get; set; }
        public string AuthToken { get; set; }
        public string PlexUri { get; set; }
        // ReSharper enable UnusedAutoPropertyAccessor.Local
    }
}
