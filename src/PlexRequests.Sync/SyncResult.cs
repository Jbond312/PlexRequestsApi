using System.Collections.Generic;
using PlexRequests.DataAccess.Dtos;

namespace PlexRequests.Sync
{
    public class SyncResult
    {
        public List<PlexMediaItemRow> NewItems { get; }
        public List<PlexMediaItemRow> ExistingItems { get; }

        public SyncResult()
        {
            NewItems = new List<PlexMediaItemRow>();
            ExistingItems = new List<PlexMediaItemRow>();
        }
    }
}
