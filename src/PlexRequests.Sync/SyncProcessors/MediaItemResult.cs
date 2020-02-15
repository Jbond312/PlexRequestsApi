
using PlexRequests.DataAccess.Dtos;

namespace PlexRequests.Sync.SyncProcessors
{
    public class MediaItemResult
    {
        public MediaItemResult(bool isNew, PlexMediaItemRow mediaItem)
        {
            IsNew = isNew;
            MediaItem = mediaItem;
        }

        public bool IsNew { get; set; }
        public PlexMediaItemRow MediaItem { get; set; }
    }
}