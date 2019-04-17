using System.Collections.Generic;
using PlexRequests.Models.SubModels.Detail;

namespace PlexRequests.Models.Plex
{
    public class SyncLibrariesCommandResult
    {
        public List<PlexServerLibraryDetailModel> Libraries { get; set; }
    }
}
