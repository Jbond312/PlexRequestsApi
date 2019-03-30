using System.Collections.Generic;
using PlexRequests.Store.Models;

namespace PlexRequests.Sync
{
    public class SyncResult
    {
        public List<PlexMediaItem> NewItems { get; set; }
        public List<PlexMediaItem> ExistingItems { get; set; }

        public SyncResult()
        {
            NewItems = new List<PlexMediaItem>();
            ExistingItems = new List<PlexMediaItem>();
        }
    }
}
