using System.Collections.Generic;
using PlexRequests.Store.Models;

namespace PlexRequests.Sync
{
    public class SyncResult
    {
        public List<PlexMediaItem> NewItems { get; }
        public List<PlexMediaItem> ExistingItems { get; }

        public SyncResult()
        {
            NewItems = new List<PlexMediaItem>();
            ExistingItems = new List<PlexMediaItem>();
        }
    }
}
