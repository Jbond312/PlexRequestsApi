using PlexRequests.Repository.Models;

namespace PlexRequests.Sync.SyncProcessors
{
    public class MediaItemResult
    {
        public MediaItemResult(bool isNew, PlexMediaItem mediaItem)
        {
            IsNew = isNew;
            MediaItem = mediaItem;
        }

        public bool IsNew { get; set; }
        public PlexMediaItem MediaItem { get; set; }
    }
}