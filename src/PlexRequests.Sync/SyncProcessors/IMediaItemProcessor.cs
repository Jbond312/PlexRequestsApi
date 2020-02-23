using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRequests.DataAccess.Dtos;
using PlexRequests.DataAccess.Enums;

namespace PlexRequests.Sync.SyncProcessors
{
    public interface IMediaItemProcessor
    {
        Task<MediaItemResult> GetMediaItem(int ratingKey, PlexMediaTypes mediaType, List<PlexMediaItemRow> localMedia, string authToken, string plexUri, string machineIdentifier, string plexUriFormat);

        void UpdateResult(SyncResult syncResult, bool isNew, PlexMediaItemRow mediaItem);
    }
}
