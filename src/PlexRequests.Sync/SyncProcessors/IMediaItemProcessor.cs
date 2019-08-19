using PlexRequests.Repository.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRequests.Repository.Enums;

namespace PlexRequests.Sync.SyncProcessors
{
    public interface IMediaItemProcessor
    {
        Task<MediaItemResult> GetMediaItem(int ratingKey, PlexMediaTypes mediaType, List<PlexMediaItem> localMedia, string authToken, string plexUri, string machineIdentifier, string plexUriFormat);

        void UpdateResult(SyncResult syncResult, bool isNew, PlexMediaItem mediaItem);
    }
}
